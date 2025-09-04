using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories.Interfaces;
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

        public async Task<User?> GetByIdAsync(int id) =>
            await _context.Users.FirstOrDefaultAsync(u => u.UID == id);

        public async Task<User?> GetUserAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
                return user;

            return null;
        }

        public async Task<User?> GetByEmailAsync(string email) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            return Task.CompletedTask;
        }

        public async Task DeleteUserAsync(int uid)
        {
            var user = await GetByIdAsync(uid);
            if (user != null)
                _context.Users.Remove(user);
        }

        // ==================== REFRESH TOKENS ====================

        public async Task AddRefreshTokenAsync(RefreshToken token)
        {
            await _context.RefreshTokens.AddAsync(token);
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == token && r.IsActive);
        }

        public Task UpdateRefreshTokenAsync(RefreshToken token)
        {
            _context.RefreshTokens.Update(token);
            return Task.CompletedTask;
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var rt = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token);
            if (rt != null && rt.Revoked == null)
            {
                rt.Revoked = DateTime.UtcNow;
                _context.RefreshTokens.Update(rt);
            }
        }

        // ==================== UNIT OF WORK ====================

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
