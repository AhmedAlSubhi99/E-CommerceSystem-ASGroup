using E_CommerceSystem.Models;

namespace E_CommerceSystem.Repositories
{
    public interface IUserRepo
    {
        void AddUser(User user);

        User? GetByEmail(string email);

        User? GetById(int id);
        void DeleteUser(int uid);
        IEnumerable<User> GetAllUsers();
        User GetUSer(string email, string password);
        User GetUserById(int uid);
        void UpdateUser(User user);
        void Update(User user);
        RefreshToken? GetRefreshToken(string token);
        void UpdateRefreshToken(RefreshToken refreshToken);
    }
}