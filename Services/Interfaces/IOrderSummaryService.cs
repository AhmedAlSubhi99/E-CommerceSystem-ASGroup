using E_CommerceSystem.Models.DTO;

namespace E_CommerceSystem.Services.Interfaces
{
    public interface IOrderSummaryService
    {
        Task<OrderSummaryDTO?> GetOrderSummaryAsync(int orderId);
        OrderSummaryDTO? GetOrderSummary(int orderId);   
        OrderSummaryDTO GetSummaryByOrderId(int orderId);

        Task<IEnumerable<OrderSummaryDTO>> GetSummariesAsync(int pageNumber = 1, int pageSize = 20);
        Task<List<OrderSummaryDTO>> GetUserOrderSummariesAsync(int userId);
    }
}
