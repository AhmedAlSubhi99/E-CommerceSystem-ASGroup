namespace E_CommerceSystem.Models
{
    public class OrdersOutputDTO
    {
        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; } = 0;

        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public string Status { get; set; } = "Pending";


    }
    public class OrderDTO
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? StatusUpdatedAtUtc { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        public string UStatus { get; set; } = default!;

    }
}
