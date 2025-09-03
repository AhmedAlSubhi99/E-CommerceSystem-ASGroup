using System.ComponentModel.DataAnnotations;

namespace E_CommerceSystem.Models
{
    public class ProductDTO
    {
        [Required]
        public string ProductName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, int.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public string? ImageUrl { get; set; }

    }
    public class ProductCreateDTO
    {
        public string ProductName { get; set; } = string.Empty;
        public IFormFile? ImageFile { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public int SupplierId { get; set; }
    }

    public class ProductUpdateDTO : ProductCreateDTO
    {
        public int ProductId { get; set; }
    }
}

