using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using System.Security.Cryptography;
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

        public UserService(IUserRepo userRepo, IConfiguration configuration, ApplicationDbContext ctx)
        {
            _userRepo = userRepo;
            _configuration = configuration;
            _ctx = ctx;
        }


        public void AddUser(User user)
        {
            _userRepo.AddUser(user);
        }
        public User? ValidateUser(string email, string password)
        {
            var user = _userRepo.GetByEmail(email);

            if (user == null) return null;

            // If you have BCrypt hashing:
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password)) return null;

            // Plaintext fallback (not secure, only if no hashing yet)
            if (user.Password != password) return null;

            return user;
        }
        public (string AccessToken, RefreshToken RefreshToken)? Login(string email, string password)
        {
            var user = ValidateUser(email, password);
            if (user == null) return null;

            // Generate JWT Access Token
            var accessToken = GenerateJwtToken(user);

            // Generate Refresh Token
            var refreshToken = GenerateRefreshToken();
            SaveRefreshToken(user.UID, refreshToken);

            return (accessToken, refreshToken);
        }

        public void DeleteUser(int uid)
        {
            var user = _userRepo.GetUserById(uid);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {uid} not found.");

            _userRepo.DeleteUser(uid);
        }
        public IEnumerable<User> GetAllUsers()
        {
            return _userRepo.GetAllUsers();
        }
        public User GetUSer(string email, string password)
        {
            var user = _userRepo.GetUSer(email, password);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }
            return user;
        }
        public User GetUserById(int uid)
        {
            var user = _userRepo.GetUserById(uid);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {uid} not found.");
            return user;
        }
        public void UpdateUser(User user)
        {
            var existingUser = _userRepo.GetUserById(user.UID);
            if (existingUser == null)
                throw new KeyNotFoundException($"User with ID {user.UID} not found.");

            _userRepo.UpdateUser(user);
        }

        public RefreshToken GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomBytes),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = "system" 
            };
        }

        public void SaveRefreshToken(int userId, RefreshToken token)
        {
            var user = _userRepo.GetById(userId);
            if (user == null) return;

            user.RefreshTokens.Add(token);
            _userRepo.Update(user);
        }

        public RefreshToken? GetRefreshToken(string token)
        {
            return _userRepo.GetRefreshToken(token);
        }

        public void RevokeRefreshToken(string token)
        {
            var refresh = _userRepo.GetRefreshToken(token);
            if (refresh != null)
            {
                refresh.Revoked = DateTime.UtcNow;
                _ctx.RefreshTokens.Update(refresh);   //  use DbContext not UserRepo
                _ctx.SaveChanges();
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
        new Claim(ClaimTypes.Role, user.Role ?? "Customer") // default role
    };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15), // Access token lifetime
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public User? GetUserByEmail(string email)
        {
            return _userRepo.GetByEmail(email);
        }
    }

}

