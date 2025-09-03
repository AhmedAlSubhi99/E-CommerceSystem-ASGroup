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

        public ReviewController(IReviewService reviewService, IConfiguration configuration, IMapper mapper)
        {
            _reviewService = reviewService;
            _configuration = configuration;
            _mapper = mapper;
        }
        [HttpPost("{productId:int}")]
        public IActionResult AddReview(int productId, [FromBody] ReviewDTO dto)
        {
            if (dto == null)
                return BadRequest("Review data is required.");

            int userId = GetUserIdFromToken();

            var review = _reviewService.AddReview(userId, productId, dto);
            var result = _mapper.Map<ReviewDTO>(review);

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("GetAllReviews")]
        public IActionResult GetAllReviews(
            [FromQuery] int productId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {

                if (pageNumber < 1 || pageSize < 1)
                    return BadRequest("PageNumber and PageSize must be greater than 0.");

                var reviews = _reviewService.GetAllReviews(pageNumber, pageSize, productId);
                // Map entities -> DTOs in one line
                var reviewDtos = _mapper.Map<List<ReviewDTO>>(reviews);

                // Prefer returning 200 with empty list rather than 404 for “no results”
                return Ok(reviewDtos);

        }

        [Authorize]
        [HttpDelete("{reviewId:int}")]
        public IActionResult DeleteReview(int reviewId)
        {
            int userId = GetUserIdFromToken();
            bool isAdmin = User.IsInRole("admin");

            _reviewService.DeleteReview(reviewId, userId, isAdmin);
            return NoContent();
        }

        [HttpPut("UpdateReview/{reviewId:int}")]
        public IActionResult UpdateReview(int reviewId, [FromBody] ReviewDTO reviewDTO)
        {
            if (reviewDTO == null) return BadRequest("Review data is required.");

            var existing = _reviewService.GetReviewById(reviewId);
            if (existing == null) return NotFound($"Review with ID {reviewId} not found.");

            var userId = GetUserIdFromToken();
            if (existing.UID != userId)
                return BadRequest("You are not authorized to update this review.");

            //  Map ReviewDTO into existing Review entity
            _mapper.Map(reviewDTO, existing);

            //  Pass the entity to the service
            _reviewService.UpdateReview(reviewId, reviewDTO);

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
            throw new UnauthorizedAccessException("Invalid or missing token.");
        }
    }
}
