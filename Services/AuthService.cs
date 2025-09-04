using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Models.DTO;
using E_CommerceSystem.Repositories.Interfaces;
using E_CommerceSystem.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace E_CommerceSystem.Services
{
    public class AuthService : IAuthService
    {
        private static readonly string[] AllowedRoles = new[] { "admin", "customer", "manager" };

        private readonly IUserRepo _userRepo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _cfg;
        private readonly ICookieTokenWriter _cookieWriter;
        private readonly IHttpContextAccessor _http;

        public AuthService(
            IUserRepo userRepo,
            IMapper mapper,
            IConfiguration cfg,
            ICookieTokenWriter cookieWriter,
            IHttpContextAccessor http)
        {
            _userRepo = userRepo;
            _mapper = mapper;
            _cfg = cfg;
            _cookieWriter = cookieWriter;
            _http = http;
        }

        // ----------------------------
        // Register
        // ----------------------------
        public async Task<UserDTO> RegisterAsync(RegisterUserDTO dto)
        {
            // Check if user already exists
            var exists = await _userRepo.GetByEmailAsync(dto.Email);
            if (exists != null)
                throw new InvalidOperationException("Email already registered.");

            // Validate role
            var role = string.IsNullOrWhiteSpace(dto.Role) ? "customer" : dto.Role.Trim();
            if (!AllowedRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Invalid role. Allowed: {string.Join(", ", AllowedRoles)}");

            // Hash password here (don’t rely on repo)
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                UName = dto.UName,
                Email = dto.Email,
                Password = hashedPassword,             
                Role = role,
                CreatedAt = DateTime.UtcNow
            };

            // Save and return mapped DTO
            await _userRepo.AddUserAsync(user);

            return _mapper.Map<UserDTO>(user);
        }


        // ----------------------------
        // Login -> issues Access + Refresh & writes cookie
        // ----------------------------
        public async Task<TokenResponseDTO> LoginAsync(LoginRequestDTO dto)
        {
            var user = await _userRepo.GetUserAsync(dto.Email, dto.Password);
            if (user == null)
                throw new UnauthorizedAccessException("Invalid credentials.");

            // Generate Access Token
            var (accessToken, accessExp) = GenerateJwt(user);

            // Generate Refresh Token
            var refreshToken = CreateRefreshToken(user.UID, days: 7);
            await _userRepo.AddRefreshTokenAsync(refreshToken);
            await _userRepo.SaveChangesAsync();

            //  Write JWT into secure HttpOnly cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,                          // cannot be accessed by JS
                Secure = true,                            // HTTPS only
                SameSite = SameSiteMode.Strict,           // prevent CSRF
                Expires = accessExp                       // match token expiration
            };

            _http.HttpContext!.Response.Cookies.Append("jwt", accessToken, cookieOptions);

            return new TokenResponseDTO
            {
                AccessToken = accessToken,
                AccessTokenExpiresAtUtc = accessExp,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiresAtUtc = refreshToken.Expires
            };
        }


        // ----------------------------
        // Refresh -> rotate refresh tokens
        // ----------------------------
        public async Task<TokenResponseDTO> RefreshAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new UnauthorizedAccessException("Missing refresh token.");

            var stored = await _userRepo.GetRefreshTokenAsync(refreshToken);
            if (stored == null || !stored.IsActive)
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            var user = stored.User ?? await _userRepo.GetByIdAsync(stored.UserId)
                       ?? throw new UnauthorizedAccessException("User not found for refresh token.");

            // rotate: revoke old, issue new
            stored.Revoked = DateTime.UtcNow;
            await _userRepo.UpdateRefreshTokenAsync(stored);

            var newRefresh = CreateRefreshToken(user.UID, days: 7);
            await _userRepo.AddRefreshTokenAsync(newRefresh);

            var (accessToken, accessExp) = GenerateJwt(user);
            _cookieWriter.Write(_http.HttpContext!.Response, accessToken, GetAccessTokenMinutes());

            return new TokenResponseDTO
            {
                AccessToken = accessToken,
                AccessTokenExpiresAtUtc = accessExp,
                RefreshToken = newRefresh.Token,
                RefreshTokenExpiresAtUtc = newRefresh.Expires
            };
        }

        // ----------------------------
        // Logout -> revoke refresh & clear cookie
        // ----------------------------
        public async Task LogoutAsync(string refreshToken)
        {
            if (!string.IsNullOrWhiteSpace(refreshToken))
                await _userRepo.RevokeRefreshTokenAsync(refreshToken);

            _cookieWriter.Clear(_http.HttpContext!.Response);
        }

        // ============================ helpers ============================

        private (string token, DateTime expiresUtc) GenerateJwt(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UID.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UID.ToString()),
                new Claim(ClaimTypes.Name, user.UName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role ?? "customer")
            };

            var minutes = GetAccessTokenMinutes();
            var exp = DateTime.UtcNow.AddMinutes(minutes);

            var token = new JwtSecurityToken(
                issuer: _cfg["Jwt:Issuer"],
                audience: _cfg["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: exp,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), exp);
        }

        private int GetAccessTokenMinutes()
        {
            if (int.TryParse(_cfg["Jwt:ExpireMinutes"], out var mins) && mins > 0)
                return mins;
            return 60;
        }

        private RefreshToken CreateRefreshToken(int userId, int days)
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(days),
                UserId = userId
            };
        }
    }
}
