using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace E_CommerceSystem.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepo _productRepo;
        private readonly IWebHostEnvironment _env;

        private readonly IMapper _mapper;



        public ProductService(IProductRepo productRepo, IWebHostEnvironment env, IMapper mapper)
        {
            _productRepo = productRepo;
            _mapper = mapper;
            _env = env;

        }


        public IEnumerable<Product> GetAllProducts(int pageNumber, int pageSize, string? name = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            // Base query
            var query = _productRepo.GetAllProducts();

            // Apply filters
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(p => p.ProductName.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // Pagination
            var pagedProducts = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return pagedProducts;
        
    }
         public Product GetProductById(int pid)
        {
            var product = _productRepo.GetProductById(pid);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {pid} not found.");
            return product;
        }

        public void AddProduct(Product product)
        {
            _productRepo.AddProduct(product);
        }

        //public async Task<string> UpdateImageAsync(int productId, IFormFile imageFile, CancellationToken ct = default)
        //{
        //    //var product = await _productRepo.GetByIdAsync(productId, ct)
        //        ?? throw new KeyNotFoundException($"Product {productId} not found.");

        //    // 1) Validate
        //    var permitted = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        //    var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        //    if (string.IsNullOrWhiteSpace(ext) || !permitted.Contains(ext))
        //        throw new InvalidOperationException("Unsupported image type. Allowed: .jpg, .jpeg, .png, .webp");
        //    if (imageFile.Length > 5 * 1024 * 1024)
        //        throw new InvalidOperationException("Image too large (max 5 MB).");

        //    // 2) Prepare paths
        //    var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
        //                                     "uploads", "products");
        //    Directory.CreateDirectory(uploadsFolder);

        //    // unique filename: productId + GUID to avoid cache collisions
        //    var fileName = $"{productId}_{Guid.NewGuid():N}{ext}";
        //    var fullPath = Path.Combine(uploadsFolder, fileName);

        //    // 3) Save new file
        //    await using (var stream = new FileStream(fullPath, FileMode.Create))
        //        await imageFile.CopyToAsync(stream, ct);

        //    // 4) Delete old image (only if it lives under our uploads folder)
        //    if (!string.IsNullOrWhiteSpace(product.ImageUrl))
        //    {
        //        // ImageUrl is stored like: /uploads/products/oldname.png
        //        var rootedOld = (product.ImageUrl.StartsWith("/"))
        //            ? Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
        //                           product.ImageUrl.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        //            : Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
        //                           product.ImageUrl.Replace("/", Path.DirectorySeparatorChar.ToString()));

        //        try
        //        {
        //            if (rootedOld.StartsWith(uploadsFolder, StringComparison.OrdinalIgnoreCase) && File.Exists(rootedOld))
        //                File.Delete(rootedOld);
        //        }
        //        catch { /* log if you have a logger; don't fail the request over cleanup */ }
        //    }

        //    // 5) Persist new relative URL and save product
        //    var relativeUrl = $"/uploads/products/{fileName}";
        //    product.ImageUrl = relativeUrl;
        //    await _productRepo.Update(product, ct);

        //    return relativeUrl;
        //}


        public void UpdateProduct(Product product)
        {

            var existingProduct = _productRepo.GetProductById(product.PID);
            if (existingProduct == null)
                throw new KeyNotFoundException($"Product with ID {product.PID} not found.");

            _productRepo.UpdateProduct(product);
        }

        public Product GetProductByName(string productName)
        {
            var product = _productRepo.GetProductByName(productName);
            if (product == null)
                throw new KeyNotFoundException($"Product with Nmae {productName} not found.");
            return product;
        }
        public (IEnumerable<ProductDTO> items, int totalCount) GetAllPaged(
    int pageNumber = 1, int pageSize = 20, string? name = null,
    decimal? minPrice = null, decimal? maxPrice = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            // Base query from repo 
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
            var p = _productRepo.GetProductById(productId)
                    ?? throw new KeyNotFoundException($"Product {productId} not found.");

            p.StockQuantity += quantity;

            _productRepo.Update(p);
            _productRepo.SaveChanges();
        }

    }
}
