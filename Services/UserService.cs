using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using System.Security.Cryptography;

namespace E_CommerceSystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepo _userRepo;

        public UserService(IUserRepo userRepo)
        {
            _userRepo = userRepo;
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
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(7) // valid for 7 days
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
                refresh.IsRevoked = true;
                _userRepo.UpdateRefreshToken(refresh);
            }
        }

    }

}

