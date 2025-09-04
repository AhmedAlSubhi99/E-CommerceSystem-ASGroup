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

        // ==================== CREATE ====================
        public async Task AddOrderProductsAsync(OrderProducts product)
        {
            try
            {
                await _orderProductsRepo.AddOrderProductsAsync(product);
                _logger.LogInformation(
                    "Added OrderProduct: OrderId={OrderId}, ProductId={ProductId}, Quantity={Quantity}",
                    product.OID, product.PID, product.Quantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error while adding OrderProduct: OrderId={OrderId}, ProductId={ProductId}",
                    product.OID, product.PID);
                throw;
            }
        }

        // ==================== READ ====================
        public async Task<IEnumerable<OrderProducts>> GetAllOrdersAsync()
        {
            var orders = await _orderProductsRepo.GetAllOrdersAsync();
            _logger.LogInformation("Fetched {Count} OrderProducts entries.", orders.Count());
            return orders;
        }

        public async Task<List<OrderProducts>> GetOrdersByOrderIdAsync(int oid)
        {
            var orders = (await _orderProductsRepo.GetByOrderIdAsync(oid)).ToList();
            if (orders.Count == 0)
            {
                _logger.LogWarning("No OrderProducts found for OrderId={OrderId}", oid);
            }
            else
            {
                _logger.LogInformation("Fetched {Count} OrderProducts for OrderId={OrderId}.", orders.Count, oid);
            }
            return orders;
        }

        public async Task<IList<OrderProducts>> GetByOrderIdWithProductAsync(int orderId)
        {
            var result = (await _orderProductsRepo.GetByOrderIdAsync(orderId, includeProduct: true)).ToList();
            _logger.LogInformation("Fetched {Count} OrderProducts with product details for OrderId={OrderId}.",
                                   result.Count, orderId);
            return result;
        }
    }
}
