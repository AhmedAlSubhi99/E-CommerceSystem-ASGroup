using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
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
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost("AddProduct")]
        public IActionResult AddProduct([FromForm] ProductCreateDTO dto)
        {
            _logger.LogInformation("AddProduct requested by {User}", User.Identity?.Name);

            var product = _mapper.Map<Product>(dto);
            _productService.AddProduct(product, dto.ImageFile);

            return Ok(new { message = "Product added successfully", product = _mapper.Map<ProductDTO>(product) });
        }

        // -------------------------------
        // Update Product
        // -------------------------------
        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("UpdateProduct/{id}")]
        public IActionResult UpdateProduct(int id, [FromForm] ProductUpdateDTO dto)
        {
            _logger.LogInformation("UpdateProduct requested for {ProductId} by {User}", id, User.Identity?.Name);

            _productService.UpdateProduct(id, dto, dto.ImageFile);
            var updated = _productService.GetProductById(id);

            return Ok(new { message = "Product updated successfully", product = _mapper.Map<ProductDTO>(updated) });
        }

        // -------------------------------
        // Get All Products with Pagination & Filtering
        // -------------------------------
        [AllowAnonymous]
        [HttpGet("GetProducts")]
        public IActionResult GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? name = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null)
        {
            if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
                return BadRequest("minPrice cannot be greater than maxPrice.");

            var products = _productService.GetProducts(page, pageSize, name, minPrice, maxPrice);
            return Ok(_mapper.Map<IEnumerable<ProductDTO>>(products));
        }

        // -------------------------------
        // Get Product by Id
        // -------------------------------
        [AllowAnonymous]
        [HttpGet("{id}")]
        public IActionResult GetProductById(int id)
        {
            var product = _productService.GetProductById(id);
            if (product == null) return NotFound();

            return Ok(_mapper.Map<ProductDTO>(product));
        }

        // -------------------------------
        // Delete Product
        // -------------------------------
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            _logger.LogInformation("DeleteProduct requested for {ProductId} by {User}", id, User.Identity?.Name);

            var product = _productService.GetProductById(id);
            if (product == null) return NotFound();

            _productService.DeleteProduct(id);
            return Ok(new { message = "Product deleted successfully" });
        }
    }
}
