using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
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
