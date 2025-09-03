namespace E_CommerceSystem.Models
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
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public List<OrderLineDTO> Lines { get; set; } = new List<OrderLineDTO>();
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class OrderStatusUpdateDTO
    {
        public string Status { get; set; } = string.Empty; // Pending, Paid, Shipped, Delivered, Cancelled
    }
}
