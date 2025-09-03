using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace E_CommerceSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[Controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<UserController> _logger;

        // ✅ Only one constructor (no DI conflicts)
        public UserController(
            IUserService userService,
            IConfiguration configuration,
            IMapper mapper,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _configuration = configuration;
            _mapper = mapper;
            _logger = logger;
        }

        // -------------------------------
        // JWT Token Generator
        // -------------------------------
        [NonAction]
        public string GenerateJwtToken(string userId, string username, string role)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Name, username),
                new Claim(ClaimTypes.Role, role ?? "Customer"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // -------------------------------
        // Register
        // -------------------------------
        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserDTO inputUser)
        {
            if (inputUser == null)
                return BadRequest("User data is required");

            // Check if email already exists
            var existing = _userService.GetUserByEmail(inputUser.Email);
            if (existing != null)
                return Conflict("A user with this email already exists.");

            var user = _mapper.Map<User>(inputUser);
            user.CreatedAt = DateTime.UtcNow;

            // ✅ Hash password before saving
            user.Password = BCrypt.Net.BCrypt.HashPassword(inputUser.Password);

            // ✅ Default role if not provided
            if (string.IsNullOrEmpty(user.Role))
                user.Role = "Customer";

            _userService.AddUser(user);

            var response = _mapper.Map<UserDTO>(user);
            return Ok(new { message = "User registered successfully", user = response });
        }

        // -------------------------------
        // Login
        // -------------------------------
        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            var user = _userService.ValidateUser(loginDto.Email, loginDto.Password);
            if (user == null)
                return Unauthorized("Invalid email or password");

            var accessToken = GenerateJwtToken(user.UID.ToString(), user.UName, user.Role);
            var refreshToken = _userService.GenerateRefreshToken();
            _userService.SaveRefreshToken(user.UID, refreshToken);

            // ✅ Store in cookies
            Response.Cookies.Append("AuthToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(30)
            });

            Response.Cookies.Append("RefreshToken", refreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = refreshToken.Expires
            });

            var response = _mapper.Map<LoginResponseDTO>(user);
            response.AccessToken = accessToken;
            response.RefreshToken = refreshToken.Token;

            return Ok(response);
        }

        // -------------------------------
        // Logout
        // -------------------------------
        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            try
            {
                Response.Cookies.Delete("AuthToken");
                Response.Cookies.Delete("RefreshToken");

                _logger.LogInformation("User {User} logged out successfully at {Time}",
                    User.Identity?.Name ?? "Unknown", DateTime.UtcNow);

                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while logging out.");
                return StatusCode(500, new { error = "An error occurred while logging out." });
            }
        }

        // -------------------------------
        // Get User by Id
        // -------------------------------
        [HttpGet("GetUserById/{UserID}")]
        public IActionResult GetUserById(int UserID)
        {
            var user = _userService.GetUserById(UserID);
            if (user == null) return NotFound();

            var dto = _mapper.Map<UserDTO>(user);
            return Ok(dto);
        }

        // -------------------------------
        // Refresh Token
        // -------------------------------
        [AllowAnonymous]
        [HttpPost("Refresh")]
        public IActionResult Refresh()
        {
            var refreshToken = Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("Refresh token missing");

            var storedToken = _userService.GetRefreshToken(refreshToken);
            if (storedToken == null || storedToken.IsExpired || storedToken.IsRevoked)
                return Unauthorized("Invalid refresh token");

            var user = _userService.GetUserById(storedToken.UserId);
            if (user == null)
                return Unauthorized("User not found");

            var newAccessToken = GenerateJwtToken(user.UID.ToString(), user.UName, user.Role);
            var newRefreshToken = _userService.GenerateRefreshToken();
            _userService.SaveRefreshToken(user.UID, newRefreshToken);

            Response.Cookies.Append("AuthToken", newAccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(30)
            });

            Response.Cookies.Append("RefreshToken", newRefreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = newRefreshToken.Expires
            });

            return Ok(new { AccessToken = newAccessToken, RefreshToken = newRefreshToken.Token });
        }
    }
}
