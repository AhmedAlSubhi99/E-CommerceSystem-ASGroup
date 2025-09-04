using E_CommerceSystem.Models.DTO;
using E_CommerceSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace E_CommerceSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(IReviewService reviewService, ILogger<ReviewController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        // ==================== GET ====================

        [AllowAnonymous]
        [HttpGet("{productId:int}")]
        public async Task<IActionResult> GetAllReviews(int productId, int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest("PageNumber and PageSize must be greater than 0.");

            var reviews = await _reviewService.GetAllReviewsAsync(pageNumber, pageSize, productId);
            return Ok(reviews);
        }

        [AllowAnonymous]
        [HttpGet("detail/{reviewId:int}")]
        public async Task<IActionResult> GetReviewById(int reviewId)
        {
            var review = await _reviewService.GetReviewByIdAsync(reviewId);
            if (review == null)
                return NotFound($"Review with ID {reviewId} not found.");
            return Ok(review);
        }

        // ==================== POST ====================

        [Authorize(Roles = "customer,admin,manager")]
        [HttpPost("{productId:int}")]
        public async Task<IActionResult> AddReview(int productId, [FromBody] ReviewCreateDTO dto)
        {
            if (dto == null)
                return BadRequest("Review data is required.");

            int userId = GetUserIdFromToken();
            var created = await _reviewService.AddReviewAsync(userId, productId, dto);

            _logger.LogInformation("User {UserId} added Review {ReviewId} for Product {ProductId}.",
                userId, created.ReviewID, productId);

            return CreatedAtAction(nameof(GetReviewById), new { reviewId = created.ReviewID }, created);
        }

        // ==================== PUT ====================

        [Authorize(Roles = "customer,admin,manager")]
        [HttpPut("{reviewId:int}")]
        public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] ReviewUpdateDTO dto)
        {
            if (dto == null)
                return BadRequest("Review data is required.");

            try
            {
                var updated = await _reviewService.UpdateReviewAsync(reviewId, dto);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // ==================== DELETE ====================

        [Authorize(Roles = "customer,admin")]
        [HttpDelete("{reviewId:int}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            try
            {
                int userId = GetUserIdFromToken();
                bool isAdmin = User.IsInRole("admin");

                await _reviewService.DeleteReviewAsync(reviewId, userId, isAdmin);

                _logger.LogInformation("Review {ReviewId} deleted by User {UserId} (Admin={IsAdmin}).",
                    reviewId, userId, isAdmin);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        // ---------------------------
        // Helper: extract userId from JWT
        // ---------------------------
        private int GetUserIdFromToken()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(token))
            {
                var jwtToken = handler.ReadJwtToken(token);
                var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub");
                if (subClaim != null && int.TryParse(subClaim.Value, out var uid))
                    return uid;
            }

            _logger.LogWarning("Invalid or missing token when trying to extract UserId.");
            throw new UnauthorizedAccessException("Invalid or missing token.");
        }
    }
}
