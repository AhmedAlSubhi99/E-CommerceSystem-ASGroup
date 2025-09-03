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
        //Task<string> UpdateImageAsync(int productId, IFormFile imageFile, CancellationToken ct = default);

        (IEnumerable<ProductDTO> items, int totalCount) GetAllPaged(
    int pageNumber = 1,
    int pageSize = 20,
    string? name = null,
    decimal? minPrice = null,
    decimal? maxPrice = null);
        void IncrementStock(int productId, int quantity);

        

    }
}