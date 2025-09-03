using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface IUserService
    {
        void AddUser(User user);
        User? ValidateUser(string email, string password);

        void DeleteUser(int uid);
        IEnumerable<User> GetAllUsers();
        User GetUSer(string email, string password);
        User GetUserById(int uid);
        void UpdateUser(User user);
        RefreshToken? ValidateRefreshToken(string token);
        RefreshToken GenerateRefreshToken(int uid);
        void SaveRefreshToken(int userId, RefreshToken token);
        RefreshToken? GetRefreshToken(string token);
        void RevokeRefreshToken(string token);
        User? GetUserByEmail(string email);
    }
}