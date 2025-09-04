using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Models.DTO;
using E_CommerceSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_CommerceSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductService productService,
            IMapper mapper,
            ILogger<ProductController> logger)
        {
            _productService = productService;
            _mapper = mapper;
            _logger = logger;
        }

        // -------------------------------
        // Add Product
        // -------------------------------
        [Authorize(Roles = "admin,manager")]
        [HttpPost("AddProduct")]
        public async Task<IActionResult> AddProduct([FromForm] ProductCreateDTO dto)
        {
            _logger.LogInformation("AddProduct requested by {User}", User.Identity?.Name);

            var product = _mapper.Map<Product>(dto);
            var created = await _productService.AddProductAsync(product, dto.ImageFile);

            return Ok(new { message = "Product added successfully", product = created });
        }

        // -------------------------------
        // Update Product
        // -------------------------------
        [Authorize(Roles = "admin,manager")]
        [HttpPut("UpdateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductUpdateDTO dto)
        {
            _logger.LogInformation("UpdateProduct requested for {ProductId} by {User}", id, User.Identity?.Name);

            var updated = await _productService.UpdateProductAsync(id, dto, dto.ImageFile);

            return Ok(new { message = "Product updated successfully", product = updated });
        }

        // -------------------------------
        // Get All Products with Pagination & Filtering
        // -------------------------------
        [AllowAnonymous]
        [HttpGet("GetProducts")]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? name = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null)
        {
            if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
                return BadRequest("minPrice cannot be greater than maxPrice.");

            var products = await _productService.GetProductsAsync(page, pageSize, name, minPrice, maxPrice);
            return Ok(_mapper.Map<IEnumerable<ProductDTO>>(products));
        }

        // -------------------------------
        // Get Product by Id
        // -------------------------------
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            return Ok(_mapper.Map<ProductDTO>(product));
        }

        // -------------------------------
        // Delete Product
        // -------------------------------
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            _logger.LogInformation("DeleteProduct requested for {ProductId} by {User}", id, User.Identity?.Name);

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            await _productService.DeleteProductAsync(id);
            return Ok(new { message = "Product deleted successfully" });
        }
    }
}
