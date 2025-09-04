using E_CommerceSystem.Models;

namespace E_CommerceSystem.Repositories
{
    public interface IProductRepo
    {
        Task AddProductAsync(Product product);
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int pid);
        Task<Product?> GetProductByNameAsync(string productName);

        void Update(Product product);
        Task<int> SaveChangesAsync();
    }
}