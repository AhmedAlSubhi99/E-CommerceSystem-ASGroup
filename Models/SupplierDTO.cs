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
        public string Name { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class SupplierUpdateDto : SupplierCreateDto { }
}
