using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Repositories
{
    public class UserRepo : IUserRepo
    {
        public ApplicationDbContext _context;
        public UserRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        //Get All users
        public IEnumerable<User> GetAllUsers()
        {
            try
            {
                return _context.Users.ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        //Get user by id
        public User GetUserById(int uid)
        {
            try
            {
                return _context.Users.FirstOrDefault(u => u.UID == uid);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }
        //Add new user
        public void AddUser(User user)
        {
            try
            {
                //Hash the password before saving
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password); 
                _context.Users.Add(user);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        //Update User 
        public void UpdateUser(User user)
        {
            try
            {
                // Only hash the password if it is updated
                if (!string.IsNullOrEmpty(user.Password))
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                }
                _context.Users.Update(user);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        //Delete User
        public void DeleteUser(int uid)
        {
            try
            {
                var user = GetUserById(uid);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        //Get user by email and passward
        public User GetUSer(string email, string password)
        {
            try
            {
                
                var user = _context.Users.Where(u => u.Email == email).FirstOrDefault();

                // Compare provided password with the hashed password
                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    return user;
                }

                return null;  //// Invalid credentials
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }
        public User? GetByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public void AddRefreshToken(RefreshToken token)
        {
            _context.RefreshTokens.Add(token);
            _context.SaveChanges();
        }

        public RefreshToken? GetRefreshToken(string token) =>
               _context.RefreshTokens.Include(r => r.User).FirstOrDefault(r => r.Token == token);

        public void RevokeRefreshToken(string token)
        {
            var rt = _context.RefreshTokens.FirstOrDefault(r => r.Token == token);
            if (rt != null)
            {
                rt.Revoked = DateTime.UtcNow;
                _context.SaveChanges();
            }
        }

        public void UpdateRefreshToken(RefreshToken token)
        {
            _context.RefreshTokens.Update(token);
            _context.SaveChanges();
        }
        public void Update(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }


        public User? GetById(int id)
        {
            return _context.Users.FirstOrDefault(u => u.UID == id);
        }


    }
}
