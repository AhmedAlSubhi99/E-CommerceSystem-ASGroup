using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface IProductService
    {
        Task<ProductDTO> AddProductAsync(Product product, IFormFile? imageFile);
        Task<ProductDTO> UpdateProductAsync(int productId, ProductUpdateDTO dto, IFormFile? imageFile);
        Task DeleteProductAsync(int productId);

        Task<IEnumerable<Product>> GetProductsAsync(int page, int pageSize, string? name, decimal? minPrice, decimal? maxPrice);
        Task<Product> GetProductByIdAsync(int pid);
        Task<Product> GetProductByNameAsync(string productName);

        Task<(IEnumerable<ProductDTO> items, int totalCount)> GetAllPagedAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? name = null,
            decimal? minPrice = null,
            decimal? maxPrice = null);

        Task IncrementStockAsync(int productId, int quantity);

        Task<string?> UploadImageAsync(int productId, IFormFile file, string uploadPath);
    }
}
