using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Repositories
{
    public class SupplierRepo : ISupplierRepo
    {
        private readonly ApplicationDbContext _ctx;
        public SupplierRepo(ApplicationDbContext ctx) => _ctx = ctx;

        public IEnumerable<Supplier> GetAll() =>
            _ctx.Suppliers.AsNoTracking().ToList();

        public Supplier? GetById(int id) =>
            _ctx.Suppliers.Include(s => s.Products).FirstOrDefault(s => s.SupplierId == id);

        public void Add(Supplier entity)
        {
            _ctx.Suppliers.Add(entity);
            _ctx.SaveChanges();
        }

        public void Update(Supplier entity)
        {
            _ctx.Suppliers.Update(entity);
            _ctx.SaveChanges();
        }

        public bool Delete(int id)
        {
            var sup = _ctx.Suppliers.FirstOrDefault(s => s.SupplierId == id);
            if (sup == null) return false;
            _ctx.Suppliers.Remove(sup);
            _ctx.SaveChanges();
            return true;
        }

        public bool Exists(int id) => _ctx.Suppliers.Any(s => s.SupplierId == id);
    }
}
