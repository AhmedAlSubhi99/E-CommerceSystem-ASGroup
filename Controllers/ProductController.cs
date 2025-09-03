using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

namespace E_CommerceSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[Controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISupplierService _supplierService;
        private readonly ApplicationDbContext _ctx;
        private readonly IMapper _mapper;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService,
            ISupplierService supplierService,
            ApplicationDbContext ctx,
            IMapper mapper)
        {
            _productService = productService;
            _categoryService = categoryService;
            _supplierService = supplierService;
            _ctx = ctx;
            _mapper = mapper;
        }

        // -------------------------------
        // Add Product
        // -------------------------------
        [Authorize(Roles = "admin")]
        [HttpPost("AddProduct")]
        public IActionResult AddProduct([FromForm] ProductCreateDTO dto)
        {
            if (dto.ImageFile != null)
            {
                var allowedTypes = new[] { "image/jpeg", "image/png" };
                if (!allowedTypes.Contains(dto.ImageFile.ContentType))
                    return BadRequest("Only JPEG and PNG images are allowed.");

                if (dto.ImageFile.Length > 2 * 1024 * 1024) // 2MB limit
                    return BadRequest("Image size cannot exceed 2MB.");
            }

            var product = _mapper.Map<Product>(dto);
            _productService.AddProduct(product, dto.ImageFile);
            return Ok(_mapper.Map<ProductDTO>(product));
        }

        // -------------------------------
        // Update Product
        // -------------------------------
        [Authorize(Roles = "admin")]
        [HttpPut("UpdateProduct/{id}")]
        public IActionResult UpdateProduct(int id, [FromForm] ProductUpdateDTO dto)
        {
            if (dto.ImageFile != null)
            {
                var allowedTypes = new[] { "image/jpeg", "image/png" };
                if (!allowedTypes.Contains(dto.ImageFile.ContentType))
                    return BadRequest("Only JPEG and PNG images are allowed.");

                if (dto.ImageFile.Length > 2 * 1024 * 1024)
                    return BadRequest("Image size cannot exceed 2MB.");
            }

            _productService.UpdateProduct(id, dto, dto.ImageFile);
            var product = _productService.GetProductById(id);
            return Ok(_mapper.Map<ProductDTO>(product));
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
            var dtos = _mapper.Map<IEnumerable<ProductDTO>>(products);
            return Ok(dtos);
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

            var dto = _mapper.Map<ProductDTO>(product);
            return Ok(dto);
        }

        // -------------------------------
        // Delete Product
        // -------------------------------
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = _productService.GetProductById(id);
            if (product == null) return NotFound();

            _productService.DeleteProduct(id);
            return NoContent();
        }
        // -------------------------------
        // Check if Category exists
        // -------------------------------
        [AllowAnonymous]
        [HttpGet("ExistsCategory/{id}")]
        public IActionResult ExistsCategory(int id)
        {
            var exists = _ctx.Categories.Any(c => c.CategoryId == id);
            return Ok(exists);
        }

        // -------------------------------
        // Check if Supplier exists
        // -------------------------------
        [AllowAnonymous]
        [HttpGet("ExistsSupplier/{id}")]
        public IActionResult ExistsSupplier(int id)
        {
            var exists = _ctx.Suppliers.Any(s => s.SupplierId == id);
            return Ok(exists);
        }


    }
}
