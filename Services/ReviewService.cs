using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepo _reviewRepo;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IOrderProductsService _orderProductsService;
        private readonly ApplicationDbContext _ctx;
        private readonly IMapper _mapper;

        public ReviewService(
            IReviewRepo reviewRepo,
            IProductService productService,
            IOrderProductsService orderProductsService,
            IOrderService orderService,
            ApplicationDbContext ctx,
            IMapper mapper)
        {
            _reviewRepo = reviewRepo;
            _productService = productService;
            _orderProductsService = orderProductsService;
            _orderService = orderService;
            _ctx = ctx;
            _mapper = mapper;
        }

        // Keep return type as in entities.
        public IEnumerable<Review> GetAllReviews(int pageNumber, int pageSize, int productId)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : pageSize;

            return _ctx.Reviews
                .Where(r => r.PID == productId)
                .OrderByDescending(r => r.ReviewDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToList();
        }

        public Review GetReviewsByProductIdAndUserId(int pid, int uid)
            => _reviewRepo.GetReviewsByProductIdAndUserId(pid, uid);

        public Review? GetReviewById(int reviewId)
            => _ctx.Reviews.FirstOrDefault(r => r.ReviewID == reviewId);

        public IEnumerable<Review> GetReviewByProductId(int pid)
            => _reviewRepo.GetReviewByProductId(pid);

        public Review AddReview(int userId, int productId, ReviewDTO dto)
        {
            //  Rule 1: Must have purchased this product
            bool purchased = _ctx.OrderProducts
                .Include(op => op.Order)
                .Any(op => op.PID == productId && op.Order.UID == userId);

            if (!purchased)
                throw new InvalidOperationException("You can only review products you have purchased.");

            //  Rule 2: Only one review per user per product
            bool alreadyReviewed = _ctx.Reviews
                .Any(r => r.PID == productId && r.UID == userId);

            if (alreadyReviewed)
                throw new InvalidOperationException("You have already reviewed this product.");

            var review = new Review
            {
                PID = productId,
                UID = userId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                ReviewDate = DateTime.UtcNow
            };

            _reviewRepo.AddReview(review);
            _ctx.SaveChanges();
            return review;
        }

        public Review UpdateReview(int rid, ReviewDTO reviewDTO)
        {
            var review = _reviewRepo.GetReviewById(rid);
            if (review == null)
                throw new KeyNotFoundException($"Review with ID {rid} not found.");

            // copy incoming fields to tracked entity
            _mapper.Map(reviewDTO, review);
            review.ReviewDate = DateTime.Now;

            _reviewRepo.UpdateReview(review);

            // Recalculate rating for this product
            RecalculateProductRating(review.PID);

            // return updated DTO
            return _mapper.Map<Review>(review);
        }

        public void DeleteReview(int reviewId, int requesterUserId, bool isAdmin)
        {
            var existing = _ctx.Reviews.FirstOrDefault(r => r.ReviewID == reviewId)
                ?? throw new KeyNotFoundException("Review not found.");

            // only admin or owner can delete
            if (!isAdmin && existing.UID != requesterUserId)
                throw new UnauthorizedAccessException("You can only delete your own reviews.");

            _ctx.Reviews.Remove(existing);
            _ctx.SaveChanges();
        }

        private void RecalculateProductRating(int pid)
        {
            // Only reviews for this product
            var reviews = _reviewRepo.GetReviewByProductId(pid).ToList();

            var product = _productService.GetProductById(pid);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {pid} not found.");

            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0.0;

            // If OverallRating is decimal in your Product model, convert safely
            product.OverallRating = Convert.ToDecimal(averageRating);

            var dto = _mapper.Map<ProductUpdateDTO>(product);
            _productService.UpdateProduct(product.PID, dto, null!);

        }
    }
}
