using E_CommerceSystem.Models;


namespace E_CommerceSystem.Repositories
{
    public interface ICategoryRepo
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
    }
}
