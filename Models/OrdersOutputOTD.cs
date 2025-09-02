namespace E_CommerceSystem.Models
{
    public class OrdersOutputOTD
    {
        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; } = 0;

        public string ProductName { get; set; }

        public int Quantity { get; set; }
    }
}
