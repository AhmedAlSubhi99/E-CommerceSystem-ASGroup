using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface ISupplierService
    {
        Task<IEnumerable<Supplier>> GetAllAsync();
        Task<Supplier?> GetByIdAsync(int id);
        Task<Supplier> CreateAsync(SupplierCreateDto dto);
        Task<bool> UpdateAsync(int id, SupplierUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
