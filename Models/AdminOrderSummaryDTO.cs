namespace E_CommerceSystem.Models
{
    public sealed class AdminOrderSummaryDTO
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalItemsSold { get; set; }
        public IEnumerable<ProductSummaryDTO> TopProducts { get; set; } = new List<ProductSummaryDTO>();
        public IEnumerable<CustomerSummaryDTO> TopCustomers { get; set; } = new List<CustomerSummaryDTO>();
    }

    public sealed class ProductSummaryDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = default!;
        public int QuantitySold { get; set; }
    }

    public sealed class CustomerSummaryDTO
    {
        public int UserId { get; set; }
        public string Name { get; set; } = default!;
        public decimal TotalSpent { get; set; }
    }
}
