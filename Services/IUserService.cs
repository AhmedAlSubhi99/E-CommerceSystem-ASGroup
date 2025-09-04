using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface IUserService
    {
        // ==================== AUTH ====================
        Task<UserDTO> RegisterAsync(UserRegisterDTO dto);
        Task<(string AccessToken, RefreshToken RefreshToken)?> LoginAsync(UserLoginDTO dto);

        // ==================== USERS ====================
        Task<UserDTO?> GetUserByIdAsync(int uid);
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task UpdateUserAsync(UserDTO dto);
        Task DeleteUserAsync(int uid);
        Task<UserDTO?> GetUserByEmailAsync(string email);

        // ==================== REFRESH TOKENS ====================
        Task<RefreshToken> GenerateRefreshTokenAsync(int userId);
        Task<RefreshToken?> ValidateRefreshTokenAsync(string token);
        Task SaveRefreshTokenAsync(int userId, RefreshToken token);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token);
    }
}
