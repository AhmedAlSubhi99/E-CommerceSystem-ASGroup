using E_CommerceSystem.Models;

namespace E_CommerceSystem.Repositories
{
    public interface ICategoryRepo
    {
        IEnumerable<Category> GetAll();
        Category? GetById(int id);
        void Add(Category entity);
        void Update(Category entity);
        bool Delete(int id);
        bool Exists(int id);
    }
}
