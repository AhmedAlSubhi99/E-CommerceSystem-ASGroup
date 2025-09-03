using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace E_CommerceSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[Controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(IReviewService reviewService, IConfiguration configuration, IMapper mapper, ILogger<ReviewController> logger)
        {
            _reviewService = reviewService;
            _configuration = configuration;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("{productId:int}")]
        public IActionResult AddReview(int productId, [FromBody] ReviewDTO dto)
        {
            if (dto == null)
            {
                _logger.LogWarning("AddReview failed: DTO was null for ProductId={ProductId}", productId);
                return BadRequest("Review data is required.");
            }

            int userId = GetUserIdFromToken();
            var review = _reviewService.AddReview(userId, productId, dto);
            var result = _mapper.Map<ReviewDTO>(review);

            _logger.LogInformation("User {UserId} added a review for ProductId={ProductId} with ReviewId={ReviewId}", userId, productId, review.ReviewID);

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("GetAllReviews")]
        public IActionResult GetAllReviews([FromQuery] int productId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                _logger.LogWarning("Invalid pagination values: PageNumber={PageNumber}, PageSize={PageSize}", pageNumber, pageSize);
                return BadRequest("PageNumber and PageSize must be greater than 0.");
            }

            var reviews = _reviewService.GetAllReviews(pageNumber, pageSize, productId);
            var reviewDtos = _mapper.Map<List<ReviewDTO>>(reviews);

            _logger.LogInformation("Fetched {Count} reviews for ProductId={ProductId}, Page={PageNumber}, PageSize={PageSize}", reviewDtos.Count, productId, pageNumber, pageSize);

            return Ok(reviewDtos);
        }

        [Authorize]
        [HttpDelete("{reviewId:int}")]
        public IActionResult DeleteReview(int reviewId)
        {
            int userId = GetUserIdFromToken();
            bool isAdmin = User.IsInRole("admin");

            _reviewService.DeleteReview(reviewId, userId, isAdmin);

            _logger.LogInformation("Review {ReviewId} deleted by User {UserId} (Admin={IsAdmin})", reviewId, userId, isAdmin);

            return NoContent();
        }

        [HttpPut("UpdateReview/{reviewId:int}")]
        public IActionResult UpdateReview(int reviewId, [FromBody] ReviewDTO reviewDTO)
        {
            if (reviewDTO == null)
            {
                _logger.LogWarning("UpdateReview failed: DTO was null for ReviewId={ReviewId}", reviewId);
                return BadRequest("Review data is required.");
            }

            var existing = _reviewService.GetReviewById(reviewId);
            if (existing == null)
            {
                _logger.LogWarning("UpdateReview failed: Review {ReviewId} not found.", reviewId);
                return NotFound($"Review with ID {reviewId} not found.");
            }

            var userId = GetUserIdFromToken();
            if (existing.UID != userId)
            {
                _logger.LogWarning("User {UserId} attempted to update Review {ReviewId} that does not belong to them.", userId, reviewId);
                return BadRequest("You are not authorized to update this review.");
            }

            _mapper.Map(reviewDTO, existing);
            _reviewService.UpdateReview(reviewId, reviewDTO);

            _logger.LogInformation("Review {ReviewId} updated successfully by User {UserId}.", reviewId, userId);

            return Ok($"Review with ID {reviewId} updated successfully.");
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
