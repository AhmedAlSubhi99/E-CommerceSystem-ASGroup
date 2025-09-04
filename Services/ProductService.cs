using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepo _productRepo;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ProductService> _logger;
        private readonly ApplicationDbContext _ctx;
        private readonly IMapper _mapper;

        private const long MaxImageSize = 2 * 1024 * 1024; // 2 MB

        public ProductService(
            IProductRepo productRepo,
            IWebHostEnvironment env,
            ILogger<ProductService> logger,
            ApplicationDbContext ctx,
            IMapper mapper)
        {
            _productRepo = productRepo;
            _mapper = mapper;
            _logger = logger;
            _ctx = ctx;
            _env = env;
        }

        // ==================== IMAGE HELPERS ====================

        private void ValidateImage(IFormFile file)
        {
            if (file.Length > MaxImageSize)
                throw new ArgumentException("File too large. Maximum allowed size is 2 MB.");

            if (!file.ContentType.StartsWith("image/"))
                throw new ArgumentException("Invalid file type. Only image files are allowed.");
        }

        private string SaveImage(IFormFile file)
        {
            var folder = Path.Combine(_env.WebRootPath ?? "wwwroot", "images");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(folder, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return "/images/" + fileName;
        }

        private void DeleteImage(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return;

            try
            {
                var filePath = Path.Combine(_env.WebRootPath ?? "wwwroot", imageUrl.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Deleted old image at {Path}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete old image {ImageUrl}", imageUrl);
            }
        }

        // ==================== CRUD OPERATIONS ====================

        public IEnumerable<Product> GetProducts(int page, int pageSize, string? name, decimal? minPrice, decimal? maxPrice)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;
            if (pageSize > 100) pageSize = 100;

            var query = _ctx.Products.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(p => EF.Functions.Like(p.ProductName, $"%{name.Trim()}%"));

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            return query
                .OrderBy(p => p.PID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public Product GetProductById(int pid)
        {
            var product = _productRepo.GetProductById(pid);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {pid} not found.");
            return product;
        }

        public void AddProduct(Product product, IFormFile? imageFile)
        {
            try
            {
                if (imageFile != null)
                {
                    ValidateImage(imageFile);
                    product.ImageUrl = SaveImage(imageFile);
                    _logger.LogInformation("Image uploaded for product {ProductName}", product.ProductName);
                }

                _ctx.Products.Add(product);
                _ctx.SaveChanges();

                _logger.LogInformation("Product {ProductName} added successfully with ID {ProductId}",
                                       product.ProductName, product.PID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding product {ProductName}", product.ProductName);
                throw;
            }
        }

        public void UpdateProduct(int productId, ProductUpdateDTO dto, IFormFile? imageFile)
        {
            try
            {
                var product = _ctx.Products.FirstOrDefault(p => p.PID == productId);
                if (product == null)
                    throw new ArgumentException("Product not found");

                _mapper.Map(dto, product);

                if (imageFile != null)
                {
                    ValidateImage(imageFile);

                    // delete old image if exists
                    DeleteImage(product.ImageUrl);

                    product.ImageUrl = SaveImage(imageFile);
                    _logger.LogInformation("Image updated for product {ProductName}", product.ProductName);
                }

                _ctx.SaveChanges();
                _logger.LogInformation("Product {ProductId} updated successfully.", productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating product {ProductId}", productId);
                throw;
            }
        }

        public void DeleteProduct(int productId)
        {
            try
            {
                var product = _ctx.Products.FirstOrDefault(p => p.PID == productId);
                if (product == null)
                    throw new KeyNotFoundException($"Product {productId} not found.");

                DeleteImage(product.ImageUrl);

                _ctx.Products.Remove(product);
                _ctx.SaveChanges();

                _logger.LogInformation("Product {ProductId} deleted successfully.", productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting Product {ProductId}", productId);
                throw;
            }
        }

        public async Task<string?> UploadImageAsync(int productId, IFormFile file, string uploadPath)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is empty.");

                ValidateImage(file);

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var imageUrl = $"/uploads/products/{fileName}";
                _logger.LogInformation("Image uploaded successfully for product {ProductId}, saved at {ImageUrl}",
                                       productId, imageUrl);

                return imageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image for product {ProductId}", productId);
                throw;
            }
        }

        // ==================== EXTRA QUERIES ====================

        public Product GetProductByName(string productName)
        {
            var product = _productRepo.GetProductByName(productName);
            if (product == null)
                throw new KeyNotFoundException($"Product with Name {productName} not found.");
            return product;
        }

        public (IEnumerable<ProductDTO> items, int totalCount) GetAllPaged(
            int pageNumber = 1, int pageSize = 20, string? name = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var q = _productRepo.GetAllProducts().AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                q = q.Where(p => p.ProductName.Contains(name));

            if (minPrice.HasValue)
                q = q.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                q = q.Where(p => p.Price <= maxPrice.Value);

            var total = q.Count();
            var page = q
                .OrderBy(p => p.PID)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = _mapper.Map<IEnumerable<ProductDTO>>(page);
            return (items, total);
        }

        public void IncrementStock(int productId, int quantity)
        {
            try
            {
                var product = _productRepo.GetProductById(productId);
                if (product == null)
                    throw new KeyNotFoundException($"Product {productId} not found.");

                product.StockQuantity += quantity;

                _productRepo.Update(product);
                _productRepo.SaveChanges();

                _logger.LogInformation("Stock incremented by {Quantity} for Product {ProductId}. New stock: {StockQuantity}",
                                       quantity, productId, product.StockQuantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while incrementing stock for Product {ProductId}", productId);
                throw;
            }
        }
    }
}
