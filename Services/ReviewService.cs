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
            return _reviewRepo.GetReviewByProductId(productId)
                              .Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize)
                              .ToList();
        }

        public Review GetReviewsByProductIdAndUserId(int pid, int uid)
            => _reviewRepo.GetReviewsByProductIdAndUserId(pid, uid);

        public ReviewDTO GetReviewById(int rid)
        {
            var review = _reviewRepo.GetReviewById(rid);
            if (review == null)
                throw new KeyNotFoundException($"Review with ID {rid} not found.");

            return _mapper.Map<ReviewDTO>(review);
        }

        public IEnumerable<Review> GetReviewByProductId(int pid)
            => _reviewRepo.GetReviewByProductId(pid);

        public Review AddReview(int userId, int productId, ReviewDTO dto)
        {
            // ✅ Rule 1: Must have purchased
            bool purchased = _ctx.OrderProducts
                .Include(op => op.Order)
                .Any(op => op.PID == productId && op.Order.UID == userId);

            if (!purchased)
                throw new InvalidOperationException("You can only review products you have purchased.");

            // ✅ Rule 2: Prevent duplicate review
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

        public ReviewDTO UpdateReview(int rid, ReviewDTO reviewDTO)
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
            return _mapper.Map<ReviewDTO>(review);
        }

        public bool DeleteReview(int rid)
        {
            var review = _reviewRepo.GetReviewById(rid);
            if (review == null) return false;

            _reviewRepo.DeleteReview(rid);

            // refresh product rating
            RecalculateProductRating(review.PID);

            return true;
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
