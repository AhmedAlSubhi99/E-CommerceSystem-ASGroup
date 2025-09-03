using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
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
        private readonly IMapper _mapper;

        public ProductController(IProductService productService, IConfiguration configuration, IMapper mapper)
        {
            _productService = productService;
            _configuration = configuration;
            _mapper = mapper;
        }

        [Authorize(Roles = "admin")]
        [RequestSizeLimit(50_000_000)] // keep upload-friendly limit
        [HttpPost("AddProduct")]
        public async Task<IActionResult> AddNewProduct([FromForm] ProductDTO productInput, IFormFile? imageFile)
        {
            if (productInput == null) return BadRequest("Product data is required.");

            if (imageFile is not null && imageFile.Length > 0)
                productInput.ImageUrl = await SaveProductImage(imageFile);

            var product = _mapper.Map<Product>(productInput);
            _productService.AddProduct(product);
            return Ok(_mapper.Map<ProductDTO>(product));
        }


        //Helper method to save the image and return its URL
        private static readonly string[] AllowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

        private async Task<string> SaveProductImage(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(ext) || !file.ContentType.StartsWith("image/"))
                throw new InvalidOperationException("Only JPG, PNG, or WEBP images are allowed.");

            const long maxBytes = 50 * 1024 * 1024; // 10 MB
            if (file.Length > maxBytes)
                throw new InvalidOperationException("File too large.");

            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
            Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var path = Path.Combine(uploadsRoot, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Public URL served by UseStaticFiles()
            return $"/uploads/products/{fileName}";
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id:int}/image")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB
        //public async Task<IActionResult> UpdateImage([FromRoute] int id, [FromForm] IFormFile file, CancellationToken ct)
        //{
        //    if (file == null || file.Length == 0)
        //        return BadRequest("No image was uploaded.");

        //    var relativeUrl = await _productService.UpdateImageAsync(id, file, ct);

        //    // If you want absolute URL:
        //    var absoluteUrl = $"{Request.Scheme}://{Request.Host}{relativeUrl}";
        //    return Ok(new { imageUrl = absoluteUrl, relativeUrl });
        //}

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
                if (product == null) return NotFound($"Product {productId} not found.");

                _mapper.Map(productInput, product);

                //product.ProductName = productInput.ProductName;
                //product.Price = productInput.Price;
                //product.Description = productInput.Description;
                //product.Stock = productInput.Stock;

                _productService.UpdateProduct(product);

                return Ok(_mapper.Map<ProductDTO>(product));
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

                var result = _mapper.Map<IEnumerable<ProductDTO>>(products);
                return Ok(result);
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

        [HttpGet("Paged")]
        [AllowAnonymous] 
        public IActionResult GetPaged(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? name = null,
    [FromQuery] decimal? minPrice = null,
    [FromQuery] decimal? maxPrice = null)
        {
            var (items, totalCount) = _productService.GetAllPaged(pageNumber, pageSize, name, minPrice, maxPrice);
            return Ok(new
            {
                pageNumber,
                pageSize,
                totalCount,
                items
            });
        }

    }
}
