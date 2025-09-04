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
            try
            {
                if (dto == null)
                {
                    _logger.LogWarning("AddProduct failed: DTO was null");
                    return BadRequest("Product data is required.");
                }

                if (dto.ImageFile != null && dto.ImageFile.Length == 0)
                    return BadRequest("Uploaded image is empty.");

                var product = _mapper.Map<Product>(dto);
                var created = await _productService.AddProductAsync(product, dto.ImageFile);

                _logger.LogInformation("Product {ProductName} created with ID {ProductId} by {User}",
                    product.ProductName, product.PID, User.Identity?.Name);

                return CreatedAtAction(nameof(GetProductById), new { id = product.PID }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding product {ProductName}", dto?.ProductName);
                return StatusCode(500, new { message = "An error occurred while adding the product.", detail = ex.Message });
            }
        }

        // -------------------------------
        // Update Product
        // -------------------------------
        [Authorize(Roles = "admin,manager")]
        [HttpPut("UpdateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductUpdateDTO dto)
        {
            try
            {
                var updated = await _productService.UpdateProductAsync(id, dto, dto.ImageFile);

                _logger.LogInformation("Product {ProductId} updated successfully by {User}", id, User.Identity?.Name);

                return Ok(new { message = "Product updated successfully", product = updated });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Product with ID {id} not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating product {ProductId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the product.", detail = ex.Message });
            }
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
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                    return NotFound(new { message = $"Product with ID {id} not found." });

                return Ok(_mapper.Map<ProductDTO>(product));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Product {ProductId}", id);
                return StatusCode(500, new { message = "An error occurred while fetching the product.", detail = ex.Message });
            }
        }

        // -------------------------------
        // Delete Product
        // -------------------------------
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                    return NotFound(new { message = $"Product with ID {id} not found." });

                await _productService.DeleteProductAsync(id);

                _logger.LogInformation("Product {ProductId} deleted by {User}", id, User.Identity?.Name);

                return Ok(new { message = "Product deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting Product {ProductId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the product.", detail = ex.Message });
            }
        }
    }
}
