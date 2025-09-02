using E_CommerceSystem.Models;

namespace E_CommerceSystem.Repositories
{
    public interface IOrderRepo
    {
        void AddOrder(Order order);
        void DeleteOrder(int oid);
        IEnumerable<Order> GetAllOrders();
        Order GetOrderById(int oid);
        void UpdateOrder(Order order);
        IEnumerable<Order> GetOrderByUserId(int uid);
    }
}