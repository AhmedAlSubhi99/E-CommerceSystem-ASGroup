using E_CommerceSystem.Models;

namespace E_CommerceSystem.Repositories
{
    public interface IUserRepo
    {
        // ==================== USER CRUD ====================
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int uid);

        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);

        // ==================== REFRESH TOKENS ====================
        Task AddRefreshTokenAsync(RefreshToken token);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task UpdateRefreshTokenAsync(RefreshToken token);
        Task RevokeRefreshTokenAsync(string token);

        // Save changes
        Task<int> SaveChangesAsync();
    }
}
