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
    public class UserController: ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IConfiguration configuration, IMapper mapper)
        {
            _userService = userService;
            _configuration = configuration;
            _mapper = mapper;
        }

        [NonAction]
        public string GenerateJwtToken(string userId, string username, string role)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Name, username),
                new Claim(JwtRegisteredClaimNames.UniqueName, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserDTO InputUser)
        {
            try
            {
                if(InputUser == null)
                    return BadRequest("User data is required");

                var user = _mapper.Map<User>(InputUser);
                user.CreatedAt = DateTime.Now;

                _userService.AddUser(user);

                return Ok(_mapper.Map<UserDTO>(user));
            }
            catch (Exception ex)
            {
                // Return a generic error response
                return StatusCode(500, $"An error occurred while adding the user. {ex.Message} ");
            }
        }


        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            var user = _userService.Authenticate(loginDto.Email, loginDto.Password);
            if (user == null)
                return Unauthorized("Invalid email or password");

            var token = _userService.GenerateJwtToken(user);

            // Store token in HttpOnly cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,           // only over HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(30) // match token lifetime
            };

            Response.Cookies.Append("AuthToken", token, cookieOptions);

            return Ok(new { message = "Login successful" });
        }

        // -------------------------------
        // Logout 
        // -------------------------------
        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            try
            {
                // Remove the cookie
                Response.Cookies.Delete("AuthToken");

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
        [HttpGet("GetUserById/{UserID}")]
        public IActionResult GetUserById(int UserID)
        {
            var user = _userService.GetUserById(UserID);
            if (user == null) return NotFound();

            var dto = _mapper.Map<UserDTO>(user);
            return Ok(dto);
        }

        


    }
}
