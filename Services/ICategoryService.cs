using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<Category> CreateAsync(CategoryCreateDTO dto);
        Task<bool> UpdateAsync(int id, CategoryUpdateDto dto);
        Task<bool> DeleteAsync(int id);

    }
}
