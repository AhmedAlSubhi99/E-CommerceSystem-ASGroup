using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services.Interfaces
{
    public interface IOrderProductsService
    {
        // ==================== CREATE ====================
        Task AddOrderProductsAsync(OrderProducts orderProduct);

        // ==================== READ ====================
        Task<IEnumerable<OrderProducts>> GetAllOrdersAsync();
        Task<List<OrderProducts>> GetOrdersByOrderIdAsync(int orderId);
        Task<IList<OrderProducts>> GetByOrderIdWithProductAsync(int orderId);
    }
}
