using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Repositories
{
    public class UserRepo : IUserRepo
    {
        private readonly ApplicationDbContext _context;

        public UserRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== USER CRUD ====================

        public async Task<IEnumerable<User>> GetAllUsersAsync() =>
            await _context.Users.AsNoTracking().ToListAsync();

        public async Task<User?> GetUserByIdAsync(int uid) =>
            await _context.Users.FirstOrDefaultAsync(u => u.UID == uid);

        public async Task AddUserAsync(User user)
        {
            if (!string.IsNullOrEmpty(user.Password))
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            if (!string.IsNullOrEmpty(user.Password))
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int uid)
        {
            var user = await GetUserByIdAsync(uid);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        // ==================== AUTH ====================

        public async Task<User?> GetUserAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
                return user;

            return null;
        }

        public async Task<User?> GetByEmailAsync(string email) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User?> GetByIdAsync(int id) =>
            await _context.Users.FirstOrDefaultAsync(u => u.UID == id);

        // ==================== REFRESH TOKENS ====================

        public async Task AddRefreshTokenAsync(RefreshToken token)
        {
            _context.RefreshTokens.Add(token);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == token && r.IsActive);
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var rt = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token);
            if (rt != null && rt.Revoked == null)
            {
                rt.Revoked = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateRefreshTokenAsync(RefreshToken token)
        {
            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
