using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace E_CommerceSystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepo _userRepo;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepo userRepo, IConfiguration configuration, IMapper mapper, ILogger<UserService> logger)
        {
            _userRepo = userRepo;
            _configuration = configuration;
            _mapper = mapper;
            _logger = logger;
        }

        // ==================== AUTH ====================

        public async Task<UserDTO> RegisterAsync(UserRegisterDTO dto)
        {
            var user = _mapper.Map<User>(dto);
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _userRepo.AddUserAsync(user);
            await _userRepo.SaveChangesAsync();

            _logger.LogInformation("User {Email} registered successfully.", user.Email);
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<(string AccessToken, RefreshToken RefreshToken)?> LoginAsync(UserLoginDTO dto)
        {
            var user = await _userRepo.GetByEmailAsync(dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                _logger.LogWarning("Login failed for {Email}.", dto.Email);
                return null;
            }

            var accessToken = GenerateJwtToken(user);
            var refreshToken = await GenerateRefreshTokenAsync(user.UID);

            await SaveRefreshTokenAsync(user.UID, refreshToken);

            _logger.LogInformation("User {Email} logged in successfully.", dto.Email);
            return (accessToken, refreshToken);
        }

        private string GenerateJwtToken(User user)
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
            return tokenHandler.WriteToken(token);
        }

        // ==================== USERS ====================

        public async Task<UserDTO?> GetUserByIdAsync(int uid)
        {
            var user = await _userRepo.GetByIdAsync(uid);
            return user != null ? _mapper.Map<UserDTO>(user) : null;
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = await _userRepo.GetAllUsersAsync();
            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }

        public async Task UpdateUserAsync(UserDTO dto)
        {
            var user = await _userRepo.GetByIdAsync(dto.UID);
            if (user == null)
            {
                _logger.LogWarning("Update failed: User {UserId} not found.", dto.UID);
                throw new KeyNotFoundException($"User with ID {dto.UID} not found.");
            }

            _mapper.Map(dto, user);
            await _userRepo.UpdateUserAsync(user);
            await _userRepo.SaveChangesAsync();

            _logger.LogInformation("User {UserId} updated successfully.", dto.UID);
        }

        public async Task DeleteUserAsync(int uid)
        {
            var user = await _userRepo.GetByIdAsync(uid);
            if (user == null)
            {
                _logger.LogWarning("Delete failed: User {UserId} not found.", uid);
                throw new KeyNotFoundException($"User with ID {uid} not found.");
            }

            await _userRepo.DeleteUserAsync(uid);
            await _userRepo.SaveChangesAsync();

            _logger.LogInformation("User {UserId} deleted successfully.", uid);
        }

        public async Task<UserDTO?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepo.GetByEmailAsync(email);
            return user != null ? _mapper.Map<UserDTO>(user) : null;
        }

        // ==================== REFRESH TOKENS ====================

        public async Task<RefreshToken> GenerateRefreshTokenAsync(int userId)
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                Expires = DateTime.UtcNow.AddDays(7),
                UserId = userId
            };

            await _userRepo.AddRefreshTokenAsync(refreshToken);
            await _userRepo.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<RefreshToken?> ValidateRefreshTokenAsync(string token)
        {
            var rt = await _userRepo.GetRefreshTokenAsync(token);
            return (rt != null && rt.IsActive) ? rt : null;
        }

        public async Task SaveRefreshTokenAsync(int userId, RefreshToken token)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return;

            user.RefreshTokens.Add(token);
            await _userRepo.UpdateUserAsync(user);
            await _userRepo.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _userRepo.GetRefreshTokenAsync(token);
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            await _userRepo.RevokeRefreshTokenAsync(token);
            await _userRepo.SaveChangesAsync();
        }
    }
}
