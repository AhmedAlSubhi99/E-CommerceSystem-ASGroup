using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace E_CommerceSystem.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepo _productRepo;

        public ProductService(IProductRepo productRepo)
        {
            _productRepo = productRepo;
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
    }
}
