using E_CommerceSystem.Models;
using System.Collections.Generic;

namespace E_CommerceSystem.Services
{
    public interface IOrderService
    {
        List<OrderProducts> GetAllOrders(int uid);
        IEnumerable<OrdersOutputDTO> GetOrderById(int oid, int uid);
        IEnumerable<Order> GetOrderByUserId(int uid);
        void DeleteOrder(int oid);

        void AddOrder(Order order);
        void UpdateOrder(Order order);
       void PlaceOrder(List<OrderItemDTO> items, int uid);
    }
}