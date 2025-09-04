using E_CommerceSystem.Models.DTO;

namespace E_CommerceSystem.Services.Interfaces
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
