namespace E_CommerceSystem.Models.DTO
{
    public class OrderItemDTO
    {
        public int ProductId { get; set; }           // FK reference (for updates/logic)
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }       // price per unit at order time
        public decimal LineTotal => Quantity * UnitPrice; // calculated field
    }
}
