using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface IOrderProductsService
    {
        void AddOrderProducts(OrderProducts product);
        IEnumerable<OrderProducts> GetAllOrders();
        List<OrderProducts> GetOrdersByOrderId(int oid);
    }
}