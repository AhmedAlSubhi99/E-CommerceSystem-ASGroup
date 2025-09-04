using E_CommerceSystem.Models.DTO;

namespace E_CommerceSystem.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserDTO> RegisterAsync(RegisterUserDTO dto);
        Task<TokenResponseDTO> LoginAsync(LoginRequestDTO dto);
        Task<TokenResponseDTO> RefreshAsync(string refreshToken);
        Task LogoutAsync(string? refreshToken);
    }
}