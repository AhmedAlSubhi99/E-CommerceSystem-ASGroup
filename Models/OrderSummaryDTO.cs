namespace E_CommerceSystem.Models
{
    public class OrderSummaryDTO
    {
        public int OrderId { get; set; }
        public string? CustomerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "Pending";

        public List<OrderLineDTO> Lines { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; } 
        public decimal Tax { get; set; }     
        public decimal Shipping { get; set; } 
        public decimal Total { get; set; }
    }

    public class OrderLineDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
    }
}
