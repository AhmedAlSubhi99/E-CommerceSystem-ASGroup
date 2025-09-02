using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface ISupplierService
    {
        Task<IEnumerable<SupplierDTO>> GetAllAsync();
        Task<SupplierDTO?> GetByIdAsync(int id);
        Task<SupplierDTO> CreateAsync(SupplierCreateDto dto);
        Task<bool> UpdateAsync(int id, SupplierUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
