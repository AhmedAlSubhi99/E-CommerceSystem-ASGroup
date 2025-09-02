using E_CommerceSystem.Models;

namespace E_CommerceSystem.Repositories
{
    public interface ISupplierRepo
    {
        Task<IEnumerable<Supplier>> GetAllAsync();
        Task<Supplier?> GetByIdAsync(int id);
        Task AddAsync(Supplier supplier);
        Task UpdateAsync(Supplier supplier);
        Task DeleteAsync(int id);
    }
}
