using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Repositories
{
    public class ProductRepo : IProductRepo
    {
        private readonly ApplicationDbContext _context;

        public ProductRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== READ ====================

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int pid)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.PID == pid);
        }

        public async Task<Product?> GetProductByNameAsync(string productName)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.ProductName == productName);
        }

        // ==================== WRITE ====================

        public async Task AddProductAsync(Product product)
        {
            if (!await _context.Categories.AnyAsync(c => c.CategoryId == product.CategoryId))
                throw new ArgumentException($"Category {product.CategoryId} not found.");

            if (!await _context.Suppliers.AnyAsync(s => s.SupplierId == product.SupplierId))
                throw new ArgumentException($"Supplier {product.SupplierId} not found.");

            await _context.Products.AddAsync(product);
        }

        public void Update(Product product)
        {
            _context.Products.Update(product);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
