using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface IProductService
    {
        Product AddProduct(ProductDTO productInput, IFormFile? imageFile);
        Product UpdateProduct(int productId, ProductDTO productInput, IFormFile? imageFile);
        IEnumerable<Product> GetAllProducts(int pageNumber, int pageSize, string? name = null, decimal? minPrice = null, decimal? maxPrice = null);
        Product GetProductById(int pid);
        Product GetProductByName(string productName);

        (IEnumerable<ProductDTO> items, int totalCount) GetAllPaged(
    int pageNumber = 1,
    int pageSize = 20,
    string? name = null,
    decimal? minPrice = null,
    decimal? maxPrice = null);
        void IncrementStock(int productId, int quantity);

        Task<string?> UploadImageAsync(int productId, IFormFile file, string uploadPath);


    }
}