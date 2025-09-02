using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface IProductService
    {
        void AddProduct(Product product);
        IEnumerable<Product> GetAllProducts(int pageNumber, int pageSize, string? name = null, decimal? minPrice = null, decimal? maxPrice = null);
        Product GetProductById(int pid);
        void UpdateProduct(Product product);
        Product GetProductByName(string productName);
    }
}