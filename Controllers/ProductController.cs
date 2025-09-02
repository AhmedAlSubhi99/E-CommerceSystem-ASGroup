using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace E_CommerceSystem.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/[Controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IConfiguration _configuration;

        public ProductController(IProductService productService, IConfiguration configuration)
        {
            _productService = productService;
            _configuration = configuration;
        }

        [HttpPost("AddProduct")]
        public IActionResult AddNewProduct(ProductDTO productInput)
        {
            try
            {
                // Retrieve the Authorization header from the request
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Decode the token to check user role
                var userRole = GetUserRoleFromToken(token);

                // Only allow Admin users to add products
                if (userRole != "admin")
                {
                    return BadRequest("You are not authorized to perform this action.");
                }

                // Check if input data is null
                if (productInput == null)
                {
                    return BadRequest("Product data is required.");
                }

                // Create a new product
                var product = new Product
                {
                    ProductName = productInput.ProductName,
                    Price = productInput.Price,
                    Description = productInput.Description,
                    Stock = productInput.Stock,
                    OverallRating = 0
                };

                // Add the new product to the database/service layer
                _productService.AddProduct(product);

                return Ok(product);
            }
            catch (Exception ex)
            {
                // Return a generic error response
                return StatusCode(500, $"An error occurred while adding the product: {ex.Message}");
            }
        }

        [HttpPut("UpdateProduct/{productId}")]
        public IActionResult UpdateProduct(int productId, ProductDTO productInput)
        {
            try
            {
                // Retrieve the Authorization header from the request
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Decode the token to check user role
                var userRole = GetUserRoleFromToken(token);

                // Only allow Admin users to add products
                if (userRole != "admin")
                {
                    return BadRequest("You are not authorized to perform this action.");
                }

                if (productInput == null)
                    return BadRequest("Product data is required.");

                var product = _productService.GetProductById(productId);
                
                product.ProductName = productInput.ProductName;
                product.Price = productInput.Price;
                product.Description = productInput.Description;
                product.Stock = productInput.Stock;
                 
                _productService.UpdateProduct(product);

                return Ok(product);
            }
            catch (Exception ex)
            {
                // Return a generic error response
                return StatusCode(500, $"An error occurred while updte product. {(ex.Message)}");
            }
        }

       
        [AllowAnonymous]
        [HttpGet("GetAllProducts")]
        public IActionResult GetAllProducts(
        [FromQuery] string? name,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            try
            {
                // Validate pagination parameters
                if (pageNumber < 1 || pageSize < 1)
                {
                    return BadRequest("PageNumber and PageSize must be greater than 0.");
                }

                // Call the service to get the paged and filtered products
                var products = _productService.GetAllProducts(pageNumber, pageSize, name, minPrice, maxPrice);

                if (products == null || !products.Any())
                {
                    return NotFound("No products found matching the given criteria.");
                }

                return Ok(products);
            }
            catch (Exception ex)
            {
                // Return a generic error response
                return StatusCode(500, $"An error occurred while retrieving products. {ex.Message}");
            }
        }

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
                // Return a generic error response
                return StatusCode(500, $"An error occurred while retrieving product. {(ex.Message)}");

            }
        }
        private string? GetUserRoleFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            if (handler.CanReadToken(token))
            {
                var jwtToken = handler.ReadJwtToken(token);

                // Extract the 'role' claim
                var roleClaim = jwtToken.Claims.FirstOrDefault (c => c.Type == "role" || c.Type == "unique_name" );
                

                return roleClaim?.Value; // Return the role or null if not found
            }

            throw new UnauthorizedAccessException("Invalid or unreadable token.");
        }
    }
}
