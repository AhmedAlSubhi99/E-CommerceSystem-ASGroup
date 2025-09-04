using System.ComponentModel.DataAnnotations;

namespace E_CommerceSystem.Models.DTO
{
    public class OrderLineDTO
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class OrderSummaryDTO
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public List<OrderLineDTO> Lines { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
    }

    public class UpdateOrderStatusDTO
    {
        [Required]
        public string Status { get; set; } = string.Empty;
        // Allowed values: Pending, Paid, Shipped, Delivered, Cancelled
    }
}
