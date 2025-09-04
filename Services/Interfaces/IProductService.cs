using E_CommerceSystem.Models;
using E_CommerceSystem.Models.DTO;

namespace E_CommerceSystem.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductDTO> AddProductAsync(Product product, IFormFile? imageFile);
        Task<ProductDTO> UpdateProductAsync(int productId, ProductUpdateDTO dto, IFormFile? imageFile);
        Task DeleteProductAsync(int productId);

        Task<IEnumerable<ProductDTO>> GetProductsAsync(int page, int pageSize, string? name, decimal? minPrice, decimal? maxPrice);
        Task<ProductDTO> GetProductByIdAsync(int pid);
        Task<ProductDTO> GetProductByNameAsync(string productName);

        Task<(IEnumerable<ProductDTO> items, int totalCount)> GetAllPagedAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? name = null,
            decimal? minPrice = null,
            decimal? maxPrice = null);

        Task IncrementStockAsync(int productId, int quantity);
    }
}
