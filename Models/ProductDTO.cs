using System.ComponentModel.DataAnnotations;

namespace E_CommerceSystem.Models
{
    // DTO for returning product info to clients
    public class ProductDTO
    {
        [Required]
        public string ProductName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        public string? ImageUrl { get; set; }
    }

    // DTO for creating a new product
    public class ProductCreateDTO
    {
        [Required]
        public string ProductName { get; set; } = string.Empty;

        public IFormFile? ImageFile { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int SupplierId { get; set; }
    }

    // DTO for updating an existing product
    public class ProductUpdateDTO : ProductCreateDTO
    {
        [Required]
        public int ProductId { get; set; }
    }
}
