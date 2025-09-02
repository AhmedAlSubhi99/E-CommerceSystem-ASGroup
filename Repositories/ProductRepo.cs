using E_CommerceSystem.Models;
using System.Security.Cryptography;

namespace E_CommerceSystem.Repositories
{
    public class ProductRepo : IProductRepo
    {
        public ApplicationDbContext _context;
        public ProductRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Product> GetAllProducts()
        {
            try
            {
                return _context.Products.ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

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
    }
}
