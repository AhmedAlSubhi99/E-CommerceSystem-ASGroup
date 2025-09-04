using E_CommerceSystem.Models;

namespace E_CommerceSystem.Repositories.Interfaces
{
    public interface ISupplierRepo
    {
        IEnumerable<Supplier> GetAll();
        public Supplier? GetById(int id);
        public void Add(Supplier entity);
        public void Update(Supplier entity);
        public bool Delete(int id);
        public bool Exists(int id);
    }
}
