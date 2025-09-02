using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Repositories
{
    public class SupplierRepo : ISupplierRepo
    {
        private readonly ApplicationDbContext _context;
        public SupplierRepo(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<Supplier>> GetAllAsync() =>
            await _context.Suppliers.ToListAsync();

        public async Task<Supplier?> GetByIdAsync(int id) =>
            await _context.Suppliers.FindAsync(id);

        public async Task AddAsync(Supplier supplier)
        {
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Supplier supplier)
        {
            _context.Suppliers.Update(supplier);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Suppliers.FindAsync(id);
            if (entity != null)
            {
                _context.Suppliers.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
