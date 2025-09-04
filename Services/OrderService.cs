using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepo _orderRepo;
        private readonly IOrderProductsRepo _orderProductsRepo;
        private readonly IProductRepo _productRepo;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepo orderRepo,
            IOrderProductsRepo orderProductsRepo,
            IProductRepo productRepo,
            IEmailService emailService,
            IMapper mapper,
            ILogger<OrderService> logger)
        {
            _orderRepo = orderRepo;
            _orderProductsRepo = orderProductsRepo;
            _productRepo = productRepo;
            _emailService = emailService;
            _mapper = mapper;
            _logger = logger;
        }

        private bool IsValidTransition(OrderStatus current, OrderStatus next)
        {
            return current switch
            {
                OrderStatus.Pending => next is OrderStatus.Paid or OrderStatus.Cancelled,
                OrderStatus.Paid => next is OrderStatus.Shipped or OrderStatus.Cancelled,
                OrderStatus.Shipped => next is OrderStatus.Delivered or OrderStatus.Cancelled,
                _ => false
            };
        }

        // ==================== CREATE ====================
        public async Task<OrderSummaryDTO> PlaceOrderAsync(List<OrderItemDTO> items, int userId)
        {
            if (items == null || !items.Any())
                throw new ArgumentException("Order must contain at least one item.");

            var order = new Order
            {
                UserId = userId,
                Status = OrderStatus.Pending,
                OrderDate = DateTime.UtcNow,
                CreatedAtUtc = DateTime.UtcNow
            };

            decimal total = 0;

            foreach (var item in items)
            {
                var product = await _productRepo.GetProductByIdAsync(item.ProductId);
                if (product == null) throw new KeyNotFoundException($"Product {item.ProductId} not found.");
                if (product.StockQuantity < item.Quantity) throw new InvalidOperationException($"{product.ProductName} out of stock.");

                product.StockQuantity -= item.Quantity;

                var orderProduct = new OrderProducts
                {
                    OID = order.OID,
                    PID = product.PID,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                };

                total += orderProduct.UnitPrice * orderProduct.Quantity;
                await _orderProductsRepo.AddOrderProductsAsync(orderProduct);
                _productRepo.Update(product);
            }

            order.TotalAmount = total;
            await _orderRepo.AddOrderAsync(order);
            await _orderRepo.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} placed successfully for user {UserId}", order.OID, userId);

            var userEmail = order.User?.Email; // if User navigation is loaded
            if (!string.IsNullOrEmpty(userEmail))
            {
                try
                {
                    await _emailService.SendOrderPlacedEmail(userEmail, order.OID, total);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send confirmation email for Order {OrderId}", order.OID);
                }
            }

            return _mapper.Map<OrderSummaryDTO>(order);
        }

        // ==================== READ ====================
        public async Task<IEnumerable<OrdersOutputDTO>> GetOrdersByUserAsync(int userId)
        {
            var orders = await _orderRepo.GetOrdersByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<OrdersOutputDTO>>(orders);
        }

        public async Task<IEnumerable<OrdersOutputDTO>> GetAllOrdersAsync(int page = 1, int pageSize = 20)
        {
            var orders = await _orderRepo.GetAllOrdersAsync();
            return orders
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => _mapper.Map<OrdersOutputDTO>(o))
                .ToList();
        }

        public async Task<OrderSummaryDTO?> GetOrderDetailsAsync(int orderId)
        {
            var order = await _orderRepo.GetOrderWithDetailsAsync(orderId);
            return order != null ? _mapper.Map<OrderSummaryDTO>(order) : null;
        }

        public async Task<Order?> GetOrderEntityByIdAsync(int orderId)
        {
            return await _orderRepo.GetOrderByIdAsync(orderId);
        }

        // ==================== UPDATE ====================
        public async Task<OrderSummaryDTO?> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto dto, int actorUserId, bool isAdminOrManager)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null) return null;

            if (!isAdminOrManager && order.UserId != actorUserId)
                throw new UnauthorizedAccessException("Not allowed to update this order.");

            var newStatus = Enum.Parse<OrderStatus>(dto.UStatus, true);
            if (!IsValidTransition(order.Status, newStatus))
                throw new InvalidOperationException($"Invalid status transition {order.Status} → {newStatus}.");

            order.Status = newStatus;
            order.StatusUpdatedAtUtc = DateTime.UtcNow;

            await _orderRepo.UpdateOrderAsync(order);
            await _orderRepo.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, order.Status);

            return _mapper.Map<OrderSummaryDTO>(order);
        }

        // ==================== DELETE ====================
        public async Task<bool> CancelOrderAsync(int orderId, int userId)
        {
            var order = await _orderRepo.GetOrderWithDetailsAsync(orderId);
            if (order == null || (order.UserId != userId && order.Status != OrderStatus.Pending))
                return false;

            foreach (var op in order.OrderProducts)
            {
                var product = await _productRepo.GetProductByIdAsync(op.PID);
                if (product != null)
                {
                    product.StockQuantity += op.Quantity;
                    _productRepo.Update(product);
                }
            }

            order.Status = OrderStatus.Cancelled;
            order.StatusUpdatedAtUtc = DateTime.UtcNow;

            await _orderRepo.UpdateOrderAsync(order);
            await _orderRepo.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} cancelled by user {UserId}", orderId, userId);

            var email = order.User?.Email;
            if (!string.IsNullOrEmpty(email))
            {
                try
                {
                    await _emailService.SendOrderCancelledEmail(email, orderId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send cancellation email for Order {OrderId}", orderId);
                }
            }

            return true;
        }
    }
}
