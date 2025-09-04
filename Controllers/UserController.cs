using AutoMapper;
using E_CommerceSystem.Models.DTO;
using E_CommerceSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_CommerceSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService,
            IMapper mapper,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _mapper = mapper;
            _logger = logger;
        }

        // -------------------------------
        // Register
        // -------------------------------
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _userService.GetUserByEmailAsync(dto.Email);
            if (existing != null)
                return Conflict("A user with this email already exists.");

            var user = await _userService.RegisterAsync(dto);
            return Ok(new { message = "User registered successfully", user });
        }

        // -------------------------------
        // Login
        // -------------------------------
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.LoginAsync(dto);
            if (result == null) return Unauthorized("Invalid credentials");

            var (accessToken, refreshToken) = result.Value;

            // Store refresh token in HttpOnly cookie
            Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = refreshToken.Expires
            });

            var user = await _userService.GetUserByEmailAsync(dto.Email);
            var response = _mapper.Map<LoginResponseDTO>(user);
            response.AccessToken = accessToken;
            response.RefreshToken = refreshToken.Token;

            return Ok(response);
        }

        // -------------------------------
        // Logout
        // -------------------------------
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("refreshToken");
            _logger.LogInformation("User {User} logged out at {Time}",
                User.Identity?.Name ?? "Unknown", DateTime.UtcNow);

            return Ok(new { message = "Logged out successfully" });
        }

        // -------------------------------
        // Get User by Id
        // -------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            return Ok(user);
        }

        // -------------------------------
        // Refresh Token
        // -------------------------------
        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var token))
                return Unauthorized("No refresh token found");

            var rt = await _userService.ValidateRefreshTokenAsync(token);
            if (rt == null) return Unauthorized("Invalid refresh token");

            var jwt = new
            {
                token = await Task.FromResult(_userService 
                    .LoginAsync(new UserLoginDTO { Email = rt.User.Email, Password = "" })) // dummy password, since refresh token is valid
            };

            return Ok(jwt);
        }
    }
}
