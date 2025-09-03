namespace E_CommerceSystem.Models
{
    public class BestSellingProductDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class RevenueByDayDTO
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
    }

    public class RevenueByMonthDTO
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopRatedProductDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public double Rating { get; set; }
        public int ReviewsCount { get; set; }
    }

    public class MostActiveCustomerDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public int OrdersCount { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
