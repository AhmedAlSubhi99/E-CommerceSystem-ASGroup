using E_CommerceSystem.Models.DTO;
using E_CommerceSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_CommerceSystem.Controllers
{
    public class RefreshRequestDTO
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService auth, ILogger<AuthController> logger)
        {
            _auth = auth;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDTO dto)
        {
            var user = await _auth.RegisterAsync(dto);
            _logger.LogInformation("User registered: {Email}", dto.Email);

            return CreatedAtAction(nameof(Register), new { email = user.Email }, user);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto)
        {
            var tokens = await _auth.LoginAsync(dto);
            _logger.LogInformation("User logged in: {Email}", dto.Email);

            return Ok(tokens);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDTO dto)
        {
            var tokens = await _auth.RefreshAsync(dto.RefreshToken);
            _logger.LogInformation("Refresh token rotated for user {Email}", User?.Identity?.Name ?? "anonymous");

            return Ok(tokens);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshRequestDTO? dto)
        {
            await _auth.LogoutAsync(dto?.RefreshToken ?? string.Empty);
            _logger.LogInformation("User logged out: {User}", User.Identity?.Name);

            return NoContent();
        }
    }
}
