using E_CommerceSystem.Models;

namespace E_CommerceSystem.Repositories.Interfaces
{
    public interface IOrderProductsRepo
    {
        // ==================== CREATE ====================
        Task AddOrderProductsAsync(OrderProducts orderProduct);

        // ==================== READ ====================
        Task<IEnumerable<OrderProducts>> GetAllOrdersAsync();
        Task<IEnumerable<OrderProducts>> GetByOrderIdAsync(int orderId, bool includeProduct = false);

        // ==================== UNIT OF WORK ====================
        Task<int> SaveChangesAsync();
    }
}
