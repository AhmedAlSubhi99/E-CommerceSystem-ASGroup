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
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(
            IReviewRepo reviewRepo,
            IProductService productService,
            IOrderProductsService orderProductsService,
            IOrderService orderService,
            ApplicationDbContext ctx,
            IMapper mapper,
            ILogger<ReviewService> logger)
        {
            _reviewRepo = reviewRepo;
            _productService = productService;
            _orderProductsService = orderProductsService;
            _orderService = orderService;
            _ctx = ctx;
            _mapper = mapper;
            _logger = logger;
        }

        public IEnumerable<Review> GetAllReviews(int pageNumber, int pageSize, int productId)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var reviews = _ctx.Reviews
                .Where(r => r.PID == productId)
                .OrderByDescending(r => r.ReviewDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToList();

            _logger.LogInformation("Fetched {Count} reviews for Product {ProductId}.", reviews.Count, productId);
            return reviews;
        }

        public Review GetReviewsByProductIdAndUserId(int pid, int uid)
            => _reviewRepo.GetReviewsByProductIdAndUserId(pid, uid);

        public Review? GetReviewById(int reviewId)
        {
            var review = _ctx.Reviews.FirstOrDefault(r => r.ReviewID == reviewId);
            if (review == null)
                _logger.LogWarning("Review {ReviewId} not found.", reviewId);
            else
                _logger.LogInformation("Fetched review {ReviewId}.", reviewId);
            return review;
        }

        public IEnumerable<Review> GetReviewByProductId(int pid)
            => _reviewRepo.GetReviewByProductId(pid);

        public Review AddReview(int userId, int productId, ReviewDTO dto)
        {
            // Rule 1
            bool purchased = _ctx.OrderProducts
                .Include(op => op.Order)
                .Any(op => op.PID == productId && op.Order.UserId == userId);

            if (!purchased)
            {
                _logger.LogWarning("User {UserId} tried to review Product {ProductId} without purchase.", userId, productId);
                throw new InvalidOperationException("You can only review products you have purchased.");
            }

            // Rule 2
            bool alreadyReviewed = _ctx.Reviews.Any(r => r.PID == productId && r.UID == userId);
            if (alreadyReviewed)
            {
                _logger.LogWarning("User {UserId} tried to add a second review for Product {ProductId}.", userId, productId);
                throw new InvalidOperationException("You have already reviewed this product.");
            }

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

            _logger.LogInformation("Review {ReviewId} added by User {UserId} for Product {ProductId}.",
                                   review.ReviewID, userId, productId);

            return review;
        }

        public Review UpdateReview(int rid, ReviewDTO reviewDTO)
        {
            var review = _reviewRepo.GetReviewById(rid);
            if (review == null)
            {
                _logger.LogWarning("Update failed: Review {ReviewId} not found.", rid);
                throw new KeyNotFoundException($"Review with ID {rid} not found.");
            }

            _mapper.Map(reviewDTO, review);
            review.ReviewDate = DateTime.Now;

            _reviewRepo.UpdateReview(review);

            RecalculateProductRating(review.PID);

            _logger.LogInformation("Review {ReviewId} updated successfully.", rid);
            return _mapper.Map<Review>(review);
        }

        public void DeleteReview(int reviewId, int requesterUserId, bool isAdmin)
        {
            var existing = _ctx.Reviews.FirstOrDefault(r => r.ReviewID == reviewId)
                ?? throw new KeyNotFoundException("Review not found.");

            if (!isAdmin && existing.UID != requesterUserId)
            {
                _logger.LogWarning("User {UserId} attempted to delete Review {ReviewId} without permission.", requesterUserId, reviewId);
                throw new UnauthorizedAccessException("You can only delete your own reviews.");
            }

            _ctx.Reviews.Remove(existing);
            _ctx.SaveChanges();

            _logger.LogInformation("Review {ReviewId} deleted by User {UserId} (Admin: {IsAdmin}).", reviewId, requesterUserId, isAdmin);
        }

        private void RecalculateProductRating(int pid)
        {
            var reviews = _reviewRepo.GetReviewByProductId(pid).ToList();
            var product = _productService.GetProductByIdAsync(pid);
            if (product == null)
            {
                _logger.LogError("Recalculate rating failed: Product {ProductId} not found.", pid);
                throw new KeyNotFoundException($"Product with ID {pid} not found.");
            }

            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0.0;
            product.Result.OverallRating = Convert.ToDecimal(averageRating);

            var dto = _mapper.Map<ProductUpdateDTO>(product);
            _productService.UpdateProductAsync(pid, dto, null).Wait();

            _logger.LogInformation("Recalculated average rating {Rating} for Product {ProductId}.", averageRating, pid);
        }
    }
}
