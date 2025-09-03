using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;

namespace E_CommerceSystem.Services
{
    public class OrderProductsService : IOrderProductsService
    {
        private readonly IOrderProductsRepo _orderProductsRepo;
        private readonly ILogger<OrderProductsService> _logger;

        public OrderProductsService(IOrderProductsRepo orderProductsRepo, ILogger<OrderProductsService> logger)
        {
            _orderProductsRepo = orderProductsRepo;
            _logger = logger;
        }

        public void AddOrderProducts(OrderProducts product)
        {
            try
            {
                _orderProductsRepo.AddOrderProducts(product);
                _logger.LogInformation("Added OrderProduct: OrderId={OrderId}, ProductId={ProductId}, Quantity={Quantity}",
                                       product.OID, product.PID, product.Quantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding OrderProduct: OrderId={OrderId}, ProductId={ProductId}",
                                 product.OID, product.PID);
                throw;
            }
        }

        public IEnumerable<OrderProducts> GetAllOrders()
        {
            var orders = _orderProductsRepo.GetAllOrders();
            _logger.LogInformation("Fetched {Count} OrderProducts entries.", orders.Count());
            return orders;
        }

        public List<OrderProducts> GetOrdersByOrderId(int oid)
        {
            var orders = _orderProductsRepo.GetOrdersByOrderId(oid);
            if (orders == null || orders.Count == 0)
            {
                _logger.LogWarning("No OrderProducts found for OrderId={OrderId}", oid);
            }
            else
            {
                _logger.LogInformation("Fetched {Count} OrderProducts for OrderId={OrderId}.", orders.Count, oid);
            }
            return orders;
        }

        public IList<OrderProducts> GetByOrderIdWithProduct(int orderId)
        {
            var result = _orderProductsRepo.GetByOrderIdWithProduct(orderId);
            _logger.LogInformation("Fetched {Count} OrderProducts with product details for OrderId={OrderId}.",
                                   result.Count, orderId);
            return result;
        }
    }
}
