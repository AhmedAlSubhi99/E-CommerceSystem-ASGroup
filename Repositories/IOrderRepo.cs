using E_CommerceSystem.Models;

namespace E_CommerceSystem.Repositories
{
    public interface IOrderRepo
    {
        // ==================== CREATE ====================
        Task AddOrderAsync(Order order);

        // ==================== READ ====================
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order?> GetOrderByIdAsync(int oid);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int uid);
        Task<Order?> GetOrderWithDetailsAsync(int orderId);

        // ==================== UPDATE ====================
        Task UpdateOrderAsync(Order order);

        // ==================== DELETE ====================
        Task DeleteOrderAsync(int oid);

        // ==================== UNIT OF WORK ====================
        Task<int> SaveChangesAsync();
    }
}
