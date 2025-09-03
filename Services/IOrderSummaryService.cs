using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface IOrderSummaryService
    {
 OrderSummaryDTO GetSummaryByOrderId(int orderId);
    IEnumerable<OrderSummaryDTO> GetSummaries(int pageNumber = 1, int pageSize = 20);
        AdminOrderSummaryDTO GetSummary(DateTime? from = null, DateTime? to = null);

        Task<OrderSummaryDTO?> GetOrderSummaryAsync(int orderId);
        OrderSummaryDTO? GetOrderSummary(int orderId);

    }
}
