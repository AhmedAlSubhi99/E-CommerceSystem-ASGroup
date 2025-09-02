using E_CommerceSystem.Models;

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
    }
}
