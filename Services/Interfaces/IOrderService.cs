using E_CommerceSystem.Models;
using E_CommerceSystem.Models.DTO;

namespace E_CommerceSystem.Services.Interfaces
{
    public interface IOrderService
    {
        // ==================== CREATE ====================
        Task<OrderSummaryDTO> PlaceOrderAsync(List<OrderItemDTO> items, int userId);

        // ==================== READ ====================
        Task<IEnumerable<OrdersOutputDTO>> GetOrdersByUserAsync(int userId);
        Task<IEnumerable<OrdersOutputDTO>> GetAllOrdersAsync(int page = 1, int pageSize = 20);
        Task<OrderSummaryDTO?> GetOrderDetailsAsync(int orderId);

        // ==================== UPDATE ====================
        Task<OrderSummaryDTO?> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto dto, int actorUserId, bool isAdminOrManager);

        // ==================== DELETE ====================
        Task<bool> CancelOrderAsync(int orderId, int userId);

        // ==================== INTERNAL (Admin/Invoice use only) ====================
        Task<Order?> GetOrderEntityByIdAsync(int orderId);
    }
}
