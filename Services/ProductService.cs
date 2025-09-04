using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Models.DTO;
using E_CommerceSystem.Repositories.Interfaces;
using E_CommerceSystem.Services.Interfaces;
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
        private readonly string _imageFolder;

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

            _imageFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "images", "products");
            if (!Directory.Exists(_imageFolder))
                Directory.CreateDirectory(_imageFolder);
        }

        // ==================== IMAGE HELPERS ====================

        private void ValidateImage(IFormFile file)
        {
            if (file.Length > MaxImageSize)
                throw new ArgumentException("File too large. Maximum allowed size is 2 MB.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                throw new ArgumentException("Only JPG and PNG images are allowed.");

            if (!file.ContentType.StartsWith("image/"))
                throw new ArgumentException("Invalid file type. Only image files are allowed.");
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(_imageFolder, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/products/{fileName}";
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

        public async Task<IEnumerable<Product>> GetProductsAsync(int page, int pageSize, string? name, decimal? minPrice, decimal? maxPrice)
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

            return await query
                .OrderBy(p => p.PID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(int pid)
        {
            var product = await _productRepo.GetProductByIdAsync(pid);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {pid} not found.");
            return product;
        }

        public async Task<ProductDTO> AddProductAsync(Product product, IFormFile? imageFile)
        {
            try
            {
                if (imageFile != null)
                {
                    ValidateImage(imageFile);
                    product.ImageUrl = await SaveImageAsync(imageFile);
                    _logger.LogInformation("Image uploaded for product {ProductName}", product.ProductName);
                }

                await _productRepo.AddProductAsync(product);
                await _productRepo.SaveChangesAsync();

                _logger.LogInformation("Product {ProductName} added successfully with ID {ProductId}",
                                       product.ProductName, product.PID);

                return _mapper.Map<ProductDTO>(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding product {ProductName}", product.ProductName);
                throw;
            }
        }

        public async Task<ProductDTO> UpdateProductAsync(int productId, ProductUpdateDTO dto, IFormFile? imageFile)
        {
            try
            {
                var product = await _ctx.Products.FirstOrDefaultAsync(p => p.PID == productId);
                if (product == null)
                    throw new ArgumentException("Product not found");

                _mapper.Map(dto, product); // Make sure AutoMapper ignores PID

                if (imageFile != null)
                {
                    ValidateImage(imageFile);
                    DeleteImage(product.ImageUrl);

                    product.ImageUrl = await SaveImageAsync(imageFile);
                    _logger.LogInformation("Image updated for product {ProductName}", product.ProductName);
                }

                await _ctx.SaveChangesAsync();
                _logger.LogInformation("Product {ProductId} updated successfully.", productId);

                return _mapper.Map<ProductDTO>(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating product {ProductId}", productId);
                throw;
            }
        }

        public async Task DeleteProductAsync(int productId)
        {
            try
            {
                var product = await _ctx.Products.FirstOrDefaultAsync(p => p.PID == productId);
                if (product == null)
                    throw new KeyNotFoundException($"Product {productId} not found.");

                DeleteImage(product.ImageUrl);

                _ctx.Products.Remove(product);
                await _ctx.SaveChangesAsync();

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

        public async Task<Product> GetProductByNameAsync(string productName)
        {
            var product = await _productRepo.GetProductByNameAsync(productName);
            if (product == null)
                throw new KeyNotFoundException($"Product with Name {productName} not found.");
            return product;
        }

        public async Task<(IEnumerable<ProductDTO> items, int totalCount)> GetAllPagedAsync(
            int pageNumber = 1, int pageSize = 20, string? name = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var q = (await _productRepo.GetAllProductsAsync()).AsQueryable();

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

        public async Task IncrementStockAsync(int productId, int quantity)
        {
            try
            {
                var product = await _productRepo.GetProductByIdAsync(productId);
                if (product == null)
                    throw new KeyNotFoundException($"Product {productId} not found.");

                product.StockQuantity += quantity;

                _productRepo.Update(product);
                await _productRepo.SaveChangesAsync();

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
