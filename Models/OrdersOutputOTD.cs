namespace E_CommerceSystem.Models
{
    public class OrdersOutputDTO
    {
        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; } = 0;

        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }
    }
}
