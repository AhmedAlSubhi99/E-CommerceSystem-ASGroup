using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface IProductService
    {
        void AddProduct(Product product, IFormFile imageFile);
        void UpdateProduct(int productId, ProductUpdateDTO dto, IFormFile imageFile);
        void DeleteProduct(int productId);
        IEnumerable<Product> GetProducts(int pageNumber, int pageSize, string? name, decimal? minPrice, decimal? maxPrice);
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