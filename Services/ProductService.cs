using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Security.Cryptography;

namespace E_CommerceSystem.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepo _productRepo;
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _ctx;
        private readonly IMapper _mapper;



        public ProductService(IProductRepo productRepo, IWebHostEnvironment env, ApplicationDbContext ctx, IMapper mapper)
        {
            _productRepo = productRepo;
            _mapper = mapper;
            _ctx = ctx;
            _env = env;

        }


        public IEnumerable<Product> GetProducts(int page, int pageSize, string name = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            var query = _ctx.Products.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(p => p.ProductName.Contains(name));

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            return query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }
        public Product GetProductById(int pid)
        {
            var product = _productRepo.GetProductById(pid);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {pid} not found.");
            return product;
        }

        public void AddProduct(Product product, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine("wwwroot/images", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                product.ImageUrl = "/images/" + fileName;
            }

            _ctx.Products.Add(product);
            _ctx.SaveChanges();
        }


        public async Task<string?> UploadImageAsync(int productId, IFormFile file, string uploadPath)
        {
            if (file == null || file.Length == 0) return null;

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/products/{fileName}";
        }

        public void UpdateProduct(int productId, ProductUpdateDTO dto, IFormFile imageFile)
        {
            var product = _ctx.Products.FirstOrDefault(p => p.PID == productId);
            if (product == null) throw new ArgumentException("Product not found");

            // map DTO
            _mapper.Map(dto, product);

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine("wwwroot/images", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                product.ImageUrl = "/images/" + fileName;
            }

            _ctx.SaveChanges();
        }

        public Product GetProductByName(string productName)
        {
            var product = _productRepo.GetProductByName(productName);
            if (product == null)
                throw new KeyNotFoundException($"Product with Name {productName} not found.");
            return product;
        }
        public (IEnumerable<ProductDTO> items, int totalCount) GetAllPaged(int pageNumber = 1, int pageSize = 20, string? name = null, decimal? minPrice = null, decimal? maxPrice = null)
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
        public void DeleteProduct(int productId)
        {
            var product = _ctx.Products.FirstOrDefault(p => p.PID == productId);
            if (product == null) throw new KeyNotFoundException($"Product {productId} not found.");
            _ctx.Products.Remove(product);
            _ctx.SaveChanges();
        }

    }
}
