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
        void UpdateRefreshToken(RefreshToken token);
        void AddRefreshToken(RefreshToken token);
        RefreshToken? GetRefreshToken(string token);
        void RevokeRefreshToken(string token);
    }
}