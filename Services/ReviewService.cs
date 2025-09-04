using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Models.DTO;
using E_CommerceSystem.Repositories.Interfaces;
using E_CommerceSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepo _reviewRepo;
        private readonly ApplicationDbContext _ctx;
        private readonly IMapper _mapper;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(
            IReviewRepo reviewRepo,
            ApplicationDbContext ctx,
            IMapper mapper,
            ILogger<ReviewService> logger)
        {
            _reviewRepo = reviewRepo;
            _ctx = ctx;
            _mapper = mapper;
            _logger = logger;
        }

        // ==================== Helpers ====================
        private async Task RecalculateProductRatingAsync(int productId)
        {
            var product = await _ctx.Products.FirstOrDefaultAsync(p => p.PID == productId);
            if (product == null)
            {
                _logger.LogError("Recalculate rating failed: Product {ProductId} not found.", productId);
                return;
            }

            var reviews = await _ctx.Reviews
                .Where(r => r.PID == productId)
                .ToListAsync();

            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0.0;
            product.OverallRating = Convert.ToDecimal(Math.Round(averageRating, 2));

            await _ctx.SaveChangesAsync();

            _logger.LogInformation("Recalculated rating {Rating} for Product {ProductId}.",
                product.OverallRating, productId);
        }

        // ==================== CRUD ====================

        public async Task<IEnumerable<ReviewDTO>> GetAllReviewsAsync(int pageNumber, int pageSize, int productId)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var reviews = await _ctx.Reviews
                .Where(r => r.PID == productId)
                .OrderByDescending(r => r.ReviewDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            _logger.LogInformation("Fetched {Count} reviews for Product {ProductId}.", reviews.Count, productId);
            return _mapper.Map<IEnumerable<ReviewDTO>>(reviews);
        }

        public async Task<ReviewDTO?> GetReviewByIdAsync(int reviewId)
        {
            var review = await _ctx.Reviews.FirstOrDefaultAsync(r => r.ReviewID == reviewId);
            if (review == null)
            {
                _logger.LogWarning("Review {ReviewId} not found.", reviewId);
                return null;
            }

            _logger.LogInformation("Fetched Review {ReviewId}.", reviewId);
            return _mapper.Map<ReviewDTO>(review);
        }

        public async Task<ReviewDTO> AddReviewAsync(int userId, int productId, ReviewCreateDTO dto)
        {
            // Ensure product was purchased
            bool purchased = await _ctx.OrderProducts
                .Include(op => op.Order)
                .AnyAsync(op => op.PID == productId && op.Order.UserId == userId);

            if (!purchased)
            {
                _logger.LogWarning("User {UserId} attempted to review Product {ProductId} without purchase.",
                    userId, productId);
                throw new InvalidOperationException("You can only review products you have purchased.");
            }

            // Ensure user hasn’t already reviewed this product
            bool alreadyReviewed = await _ctx.Reviews.AnyAsync(r => r.PID == productId && r.UID == userId);
            if (alreadyReviewed)
            {
                _logger.LogWarning("User {UserId} attempted a duplicate review for Product {ProductId}.",
                    userId, productId);
                throw new InvalidOperationException("You have already reviewed this product.");
            }

            var review = _mapper.Map<Review>(dto);
            review.UID = userId;
            review.PID = productId;
            review.ReviewDate = DateTime.UtcNow;

            await _ctx.Reviews.AddAsync(review);
            await _ctx.SaveChangesAsync();

            await RecalculateProductRatingAsync(productId);

            _logger.LogInformation("Review {ReviewId} added by User {UserId} for Product {ProductId}.",
                review.ReviewID, userId, productId);

            return _mapper.Map<ReviewDTO>(review);
        }

        public async Task<ReviewDTO> UpdateReviewAsync(int reviewId, ReviewUpdateDTO dto)
        {
            var review = await _ctx.Reviews.FirstOrDefaultAsync(r => r.ReviewID == reviewId);
            if (review == null)
            {
                _logger.LogWarning("Update failed: Review {ReviewId} not found.", reviewId);
                throw new KeyNotFoundException($"Review with ID {reviewId} not found.");
            }

            _mapper.Map(dto, review);
            review.ReviewDate = DateTime.UtcNow;

            await _ctx.SaveChangesAsync();

            await RecalculateProductRatingAsync(review.PID);

            _logger.LogInformation("Review {ReviewId} updated successfully.", reviewId);
            return _mapper.Map<ReviewDTO>(review);
        }

        public async Task DeleteReviewAsync(int reviewId, int requesterUserId, bool isAdmin)
        {
            var review = await _ctx.Reviews.FirstOrDefaultAsync(r => r.ReviewID == reviewId)
                ?? throw new KeyNotFoundException("Review not found.");

            if (!isAdmin && review.UID != requesterUserId)
            {
                _logger.LogWarning("User {UserId} attempted to delete Review {ReviewId} without permission.",
                    requesterUserId, reviewId);
                throw new UnauthorizedAccessException("You can only delete your own reviews.");
            }

            int productId = review.PID;

            _ctx.Reviews.Remove(review);
            await _ctx.SaveChangesAsync();

            await RecalculateProductRatingAsync(productId);

            _logger.LogInformation("Review {ReviewId} deleted by User {UserId} (Admin: {IsAdmin}).",
                reviewId, requesterUserId, isAdmin);
        }
    }
}
