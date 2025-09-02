using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore; 
using System.Threading;
using System.Threading.Tasks;

namespace E_CommerceSystem.Repositories
{
    public class ProductRepo : IProductRepo
    {
        public ApplicationDbContext _context;
        public ProductRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Product> GetAllProducts() => _context.Products.ToList();


        public Product GetProductById(int pid)
        {
            try
            {
                return _context.Products.FirstOrDefault(p => p.PID == pid);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        public void AddProduct(Product product)
        {
            try
            {
                if (!_context.Categories.Any(c => c.CategoryId == product.CategoryId))
                    throw new ArgumentException($"Category {product.CategoryId} not found.");

                if (!_context.Suppliers.Any(s => s.SupplierId == product.SupplierId))
                    throw new ArgumentException($"Supplier {product.SupplierId} not found.");

                _context.Products.Add(product);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        public void UpdateProduct(Product product)
        {
            try
            {
                _context.Products.Update(product);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        public Product GetProductByName( string productName)
        {
            try
            {
                return _context.Products.FirstOrDefault(p => p.ProductName == productName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
           
        }

        public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            // No AsNoTracking so entity stays tracked for updates
            return await _context.Products.FirstOrDefaultAsync(p => p.PID == id, ct);
        }

        public async Task UpdateAsync(Product product, CancellationToken ct = default)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync(ct);
        }
        public void Update(Product product)
        {
            _context.Products.Update(product);
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }
    }
}
