using System.ComponentModel.DataAnnotations;

namespace E_CommerceSystem.Models
{
    public class SupplierDTO
    {
        public int SupplierId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class SupplierCreateDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string ContactEmail { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;
    }

    public class SupplierUpdateDto : SupplierCreateDto { }
}
