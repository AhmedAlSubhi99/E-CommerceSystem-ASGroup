using E_CommerceSystem.Models;
using System.Collections.Generic;

namespace E_CommerceSystem.Services
{
    public interface IOrderService
    {
        // Returns user’s orders with their products (existing contract)
        List<OrderProducts> GetAllOrders(int uid);

        // Single-order DTO projection (typo fixed to DTO)
        IEnumerable<OrdersOutputDTO> GetOrderById(int oid, int uid);

        // Raw Order entities for a user (existing contract)
        IEnumerable<Order> GetOrderByUserId(int uid);

        // All orders for a user as DTOs (existing contract)
        IEnumerable<OrdersOutputDTO> GetAllOrdersDto(int uid);

        // Entity fetch (needed by OrderSummaryService)
        Order? GetOrderEntityById(int oid);

        IEnumerable<Order> GetAllOrders();
        OrderSummaryDTO GetOrderSummary(int orderId);

        public void UpdateOrderStatus(int orderId, OrderStatus newStatus);

        // Mutations
        void AddOrder(Order order);
        void UpdateOrder(Order order);
        void DeleteOrder(int oid);
        bool UpdateStatus(int orderId, OrderStatus newStatus);

        // Place order (keep one signature)
        Task PlaceOrder(List<OrderItemDTO> items, int uid);

        // Cancel an order (owner or admin). Restores stock. Returns (ok, message).
        Task CancelOrder(int orderId, int uid);
        Task<OrderSummaryDTO?> GetOrderDetails(int orderId);
        OrderDTO SetStatus(int orderId, OrderStatus newStatus, int actorUserId, bool isAdminOrManager);
    }
}
