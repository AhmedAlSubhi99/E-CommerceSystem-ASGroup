using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface ICategoryService
    {
        IEnumerable<CategoryDTO> GetAll();
        CategoryDTO? GetById(int id);
        CategoryDTO Create(CategoryDTO input);
        bool Update(int id, CategoryDTO input);
        bool Delete(int id);
    }
}
