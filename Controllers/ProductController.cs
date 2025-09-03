using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace E_CommerceSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[Controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _ctx;
        private readonly IMapper _mapper;

        public ProductController(IProductService productService, IConfiguration configuration, ApplicationDbContext ctx, IMapper mapper)
        {
            _productService = productService;
            _configuration = configuration;
            _ctx = ctx;
            _mapper = mapper;
        }

        // =============================
        // ADD PRODUCT (with optional image)
        // =============================
        [Authorize(Roles = "admin")]
        [RequestSizeLimit(50_000_000)]
        [HttpPost("AddProduct")]
        public IActionResult AddNewProduct([FromForm] ProductDTO productInput, IFormFile? imageFile)
        {
            if (productInput == null)
                return BadRequest("Product data is required.");

            var product = _productService.AddProduct(productInput, imageFile);
            return Ok(_mapper.Map<ProductDTO>(product));
        }

        // =============================
        // UPDATE/REPLACE PRODUCT IMAGE
        // =============================
        [HttpPut("UpdateProduct/ {productId}")]
        public IActionResult UpdateProduct(int productId, [FromForm] ProductDTO productInput, IFormFile? imageFile)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var userRole = GetUserRoleFromToken(token);

                if (userRole != "admin")
                    return BadRequest("You are not authorized to perform this action.");

                if (productInput == null)
                    return BadRequest("Product data is required.");

                var product = _productService.UpdateProduct(productId, productInput, imageFile);
                return Ok(_mapper.Map<ProductDTO>(product));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating product. {ex.Message}");
            }
        }

        // =============================
        // UPDATE PRODUCT INFO (no image)
        // =============================
        [HttpPut("UpdateProduct/{productId}")]
        public IActionResult UpdateProduct(int productId, ProductDTO productInput)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var userRole = GetUserRoleFromToken(token);

                if (userRole != "admin")
                    return BadRequest("You are not authorized to perform this action.");

                if (productInput == null)
                    return BadRequest("Product data is required.");

                var product = _productService.GetProductById(productId);
                if (product == null) return NotFound($"Product {productId} not found.");

                _mapper.Map(productInput, product);
                _ctx.Products.Update(product);
                _ctx.SaveChangesAsync();
                return Ok(_mapper.Map<ProductDTO>(product));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating product. {ex.Message}");
            }
        }

        // =============================
        // GET PRODUCTS (paged + filtered)
        // =============================
        [AllowAnonymous]
        [HttpGet("GetAllProducts")]
        public IActionResult GetAllProducts([FromQuery] string? name, [FromQuery] decimal? minPrice,
                                            [FromQuery] decimal? maxPrice, [FromQuery] int pageNumber = 1,
                                            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1 || pageSize < 1)
                    return BadRequest("PageNumber and PageSize must be greater than 0.");

                var products = _productService.GetAllProducts(pageNumber, pageSize, name, minPrice, maxPrice);

                if (products == null || !products.Any())
                    return NotFound("No products found matching the given criteria.");

                var result = _mapper.Map<IEnumerable<ProductDTO>>(products);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving products. {ex.Message}");
            }
        }

        // =============================
        // GET PRODUCT BY ID
        // =============================
        [AllowAnonymous]
        [HttpGet("GetProductByID/{ProductId}")]
        public IActionResult GetProductById(int ProductId)
        {
            try
            {
                var product = _productService.GetProductById(ProductId);
                if (product == null)
                    return NotFound("No product found.");

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving product. {ex.Message}");
            }
        }

        // =============================
        // GET PAGED (alternative)
        // =============================
        [HttpGet("Paged")]
        [AllowAnonymous]
        public IActionResult GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20,
                                      [FromQuery] string? name = null, [FromQuery] decimal? minPrice = null,
                                      [FromQuery] decimal? maxPrice = null)
        {
            var (items, totalCount) = _productService.GetAllPaged(pageNumber, pageSize, name, minPrice, maxPrice);
            return Ok(new { pageNumber, pageSize, totalCount, items });
        }

        // =============================
        // JWT Helper
        // =============================
        private string? GetUserRoleFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            if (handler.CanReadToken(token))
            {
                var jwtToken = handler.ReadJwtToken(token);
                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == "unique_name");
                return roleClaim?.Value;
            }

            throw new UnauthorizedAccessException("Invalid or unreadable token.");
        }
    }
}
