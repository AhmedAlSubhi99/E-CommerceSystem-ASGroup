using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface IAuthService
    {
        Task<UserDTO> RegisterAsync(RegisterUserDTO dto);
        Task<TokenResponseDTO> LoginAsync(LoginRequestDTO dto);
        Task<TokenResponseDTO> RefreshAsync(string refreshToken);
        Task LogoutAsync(string? refreshToken);
    }
}