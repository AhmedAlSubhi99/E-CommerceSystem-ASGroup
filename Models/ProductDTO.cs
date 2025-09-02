using System.ComponentModel.DataAnnotations;

namespace E_CommerceSystem.Models
{
    public class ProductDTO
    {
        [Required]
        public string ProductName { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }
}
