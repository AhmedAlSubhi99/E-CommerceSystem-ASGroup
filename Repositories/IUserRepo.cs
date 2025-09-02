using E_CommerceSystem.Models;

namespace E_CommerceSystem.Repositories
{
    public interface IUserRepo
    {
        void AddUser(User user);
        void DeleteUser(int uid);
        IEnumerable<User> GetAllUsers();
        User GetUSer(string email, string password);
        User GetUserById(int uid);
        void UpdateUser(User user);
    }
}