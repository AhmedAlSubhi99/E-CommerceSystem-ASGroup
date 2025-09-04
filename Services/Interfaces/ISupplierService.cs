using E_CommerceSystem.Models.DTO;

namespace E_CommerceSystem.Services.Interfaces
{
    public interface ISupplierService
    {
        IEnumerable<SupplierDTO> GetAll();
        public SupplierDTO? GetById(int id);
        public SupplierDTO Create(SupplierDTO input);
        public bool Update(int id, SupplierDTO input);
        public bool Delete(int id);
    }
}
