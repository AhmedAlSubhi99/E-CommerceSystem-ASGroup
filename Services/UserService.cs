using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace E_CommerceSystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepo _userRepo;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _ctx;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepo userRepo, IConfiguration configuration, ApplicationDbContext ctx, ILogger<UserService> logger)
        {
            _userRepo = userRepo;
            _configuration = configuration;
            _ctx = ctx;
            _logger = logger;
        }

        public void AddUser(User user)
        {
            _userRepo.AddUser(user);
            _logger.LogInformation("User {UserEmail} registered successfully with ID {UserId}.", user.Email, user.UID);
        }

        public User? ValidateUser(string email, string password)
        {
            var user = _userRepo.GetByEmail(email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: user {Email} not found.", email);
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                _logger.LogWarning("Login failed: invalid password for {Email}.", email);
                return null;
            }

            _logger.LogInformation("User {Email} validated successfully.", email);
            return user;
        }

        public (string AccessToken, RefreshToken RefreshToken)? Login(string email, string password)
        {
            var user = ValidateUser(email, password);
            if (user == null)
            {
                _logger.LogWarning("Login attempt failed for {Email}.", email);
                return null;
            }

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken(user.UID);
            SaveRefreshToken(user.UID, refreshToken);

            _logger.LogInformation("User {Email} logged in successfully. Refresh token generated.", email);
            return (accessToken, refreshToken);
        }

        public void DeleteUser(int uid)
        {
            var user = _userRepo.GetUserById(uid);
            if (user == null)
            {
                _logger.LogWarning("Delete failed: User {UserId} not found.", uid);
                throw new KeyNotFoundException($"User with ID {uid} not found.");
            }

            _userRepo.DeleteUser(uid);
            _logger.LogInformation("User {UserId} deleted successfully.", uid);
        }

        public IEnumerable<User> GetAllUsers()
        {
            var users = _userRepo.GetAllUsers();
            _logger.LogInformation("Fetched {Count} users from database.", users.Count());
            return users;
        }

        public User GetUSer(string email, string password)
        {
            var user = _userRepo.GetUSer(email, password);
            if (user == null)
            {
                _logger.LogWarning("GetUser failed: invalid credentials for {Email}.", email);
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            _logger.LogInformation("User {Email} retrieved successfully via GetUser.", email);
            return user;
        }

        public User GetUserById(int uid)
        {
            var user = _userRepo.GetUserById(uid);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found.", uid);
                throw new KeyNotFoundException($"User with ID {uid} not found.");
            }

            _logger.LogInformation("Fetched user with ID {UserId}.", uid);
            return user;
        }

        public void UpdateUser(User user)
        {
            var existingUser = _userRepo.GetUserById(user.UID);
            if (existingUser == null)
            {
                _logger.LogWarning("Update failed: User {UserId} not found.", user.UID);
                throw new KeyNotFoundException($"User with ID {user.UID} not found.");
            }

            _userRepo.UpdateUser(user);
            _logger.LogInformation("User {UserId} updated successfully.", user.UID);
        }

        public RefreshToken GenerateRefreshToken(int userId)
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                Expires = DateTime.UtcNow.AddDays(7),
                UserId = userId
            };

            _userRepo.AddRefreshToken(refreshToken);
            _logger.LogInformation("Refresh token generated for User {UserId}.", userId);
            return refreshToken;
        }

        public RefreshToken? ValidateRefreshToken(string token)
        {
            var rt = _userRepo.GetRefreshToken(token);
            if (rt == null || !rt.IsActive)
            {
                _logger.LogWarning("Invalid or inactive refresh token used.");
                return null;
            }

            _logger.LogInformation("Refresh token validated for User {UserId}.", rt.UserId);
            return rt;
        }

        public void SaveRefreshToken(int userId, RefreshToken token)
        {
            var user = _userRepo.GetById(userId);
            if (user == null)
            {
                _logger.LogWarning("Failed to save refresh token: User {UserId} not found.", userId);
                return;
            }

            user.RefreshTokens.Add(token);
            _userRepo.Update(user);

            _logger.LogInformation("Refresh token saved for User {UserId}.", userId);
        }

        public RefreshToken? GetRefreshToken(string token)
        {
            var rt = _userRepo.GetRefreshToken(token);
            if (rt == null)
                _logger.LogWarning("Requested refresh token not found.");
            else
                _logger.LogInformation("Fetched refresh token for User {UserId}.", rt.UserId);

            return rt;
        }

        public void RevokeRefreshToken(string token)
        {
            var refresh = _userRepo.GetRefreshToken(token);
            if (refresh != null)
            {
                refresh.Revoked = DateTime.UtcNow;
                _ctx.RefreshTokens.Update(refresh);
                _ctx.SaveChanges();

                _logger.LogInformation("Refresh token revoked for User {UserId}.", refresh.UserId);
            }
            else
            {
                _logger.LogWarning("Attempt to revoke non-existent refresh token.");
            }
        }

        public string GenerateJwtToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UID.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role ?? "Customer")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            _logger.LogInformation("JWT access token generated for User {UserId}.", user.UID);

            return tokenHandler.WriteToken(token);
        }

        public User? GetUserByEmail(string email)
        {
            var user = _userRepo.GetByEmail(email);
            if (user == null)
                _logger.LogWarning("User with email {Email} not found.", email);
            else
                _logger.LogInformation("Fetched user by email {Email}.", email);

            return user;
        }
    }
}
