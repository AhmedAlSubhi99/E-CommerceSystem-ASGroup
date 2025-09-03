using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepo _orderRepo;
        private readonly IProductService _productService;
        private readonly IOrderProductsService _orderProductsService;
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _ctx;
        private readonly IMapper _mapper;

        public OrderService(
            IOrderRepo orderRepo,
            IProductService productService,
            IOrderProductsService orderProductsService,
            IEmailService emailService,
            ApplicationDbContext ctx,
            IMapper mapper)
        {
            _orderRepo = orderRepo;
            _productService = productService;
            _orderProductsService = orderProductsService;
            _emailService = emailService;
            _ctx = ctx;
            _mapper = mapper;
        }
        private static bool IsValidTransition(OrderStatus from, OrderStatus to)
        {
            return (from, to) switch
            {
                (OrderStatus.Pending, OrderStatus.Paid) => true,
                (OrderStatus.Pending, OrderStatus.Cancelled) => true,
                (OrderStatus.Paid, OrderStatus.Shipped) => true,
                (OrderStatus.Shipped, OrderStatus.Delivered) => true,
                // Disallow regressions & skipping critical steps
                _ => false
            };
        }
        public OrderDTO SetStatus(int orderId, OrderStatus newStatus, int actorUserId, bool isAdminOrManager)
        {
            var order = _orderRepo.GetOrderById(orderId)
                        ?? throw new KeyNotFoundException($"Order {orderId} not found.");

            if (!isAdminOrManager)
            {
                if (newStatus != OrderStatus.Cancelled ||
                    order.UID != actorUserId ||
                    order.Status != OrderStatus.Pending)
                {
                    throw new UnauthorizedAccessException("You are not allowed to change this order status.");
                }
            }

            if (!IsValidTransition(order.Status, newStatus))
                throw new InvalidOperationException($"Invalid status transition {order.Status} → {newStatus}.");

            order.Status = newStatus;
            order.StatusUpdatedAtUtc = DateTime.UtcNow;

            try
            {
                _orderRepo.UpdateOrder(order);
                _orderRepo.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException("This order was modified by another user. Please reload and try again.");
            }

            return _mapper.Map<OrderDTO>(order);
        }

        public bool UpdateStatus(int orderId, OrderStatus newStatus)
        {
            var order = _ctx.Orders.FirstOrDefault(o => o.OID == orderId);
            if (order == null) return false;

            // If already closed (Cancelled or Delivered), block further changes
            if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Delivered)
                return false;

            // Allowed transitions
            bool valid = order.Status switch
            {
                OrderStatus.Pending => newStatus == OrderStatus.Paid || newStatus == OrderStatus.Cancelled,
                OrderStatus.Paid => newStatus == OrderStatus.Shipped || newStatus == OrderStatus.Cancelled,
                OrderStatus.Shipped => newStatus == OrderStatus.Delivered,
                _ => false
            };

            if (!valid) return false;

            order.Status = newStatus;
            _ctx.SaveChanges();
            return true;
        }


        // Fetches ALL order lines (OrderProducts) for a given user
        public List<OrderProducts> GetAllOrders(int uid)
        {
            var orders = _orderRepo.GetOrderByUserId(uid) ?? Enumerable.Empty<Order>();
            var allOrderProducts = new List<OrderProducts>();

            foreach (var order in orders)
            {
                var lines = _orderProductsService.GetOrdersByOrderId(order.OID);
                if (lines != null) allOrderProducts.AddRange(lines);
            }

            return allOrderProducts;
        }


        public IEnumerable<Order> GetAllOrders()
        {
            return _orderRepo.GetAllOrders(); // repo should return IEnumerable<Order>
        }


        public IEnumerable<OrdersOutputDTO> GetAllOrdersDto(int uid)
        {
            var lines = GetAllOrders(uid); // List<OrderProducts>
            return _mapper.Map<List<OrdersOutputDTO>>(lines);
        }

        public IEnumerable<OrdersOutputDTO> GetOrderById(int oid, int uid)
        {
            var order = _orderRepo.GetOrderById(oid);
            if (order == null || order.UID != uid)
                return Enumerable.Empty<OrdersOutputDTO>();

            // Prefer the repo/service that loads Product with the line if you have it
            var linesWithProducts = _orderProductsService.GetByOrderIdWithProduct(oid)
                                   ?? _orderProductsService.GetOrdersByOrderId(oid)
                                   ?? Enumerable.Empty<OrderProducts>();

            return _mapper.Map<List<OrdersOutputDTO>>(linesWithProducts);
        }

        public IEnumerable<Order> GetOrderByUserId(int uid)
        {
            var orders = _orderRepo.GetOrderByUserId(uid);
            if (orders == null)
                throw new KeyNotFoundException($"Orders for user {uid} not found.");
            return orders;
        }

        public Order? GetOrderEntityById(int oid) => _orderRepo.GetOrderById(oid);

        public void DeleteOrder(int oid)
        {
            var order = _orderRepo.GetOrderById(oid)
                        ?? throw new KeyNotFoundException($"Order {oid} not found.");
            _orderRepo.DeleteOrder(oid);
        }

        public void AddOrder(Order order) => _orderRepo.AddOrder(order);
        public void UpdateOrder(Order order) => _orderRepo.UpdateOrder(order);

        public async Task PlaceOrder(List<OrderItemDTO> items, int uid)
        {
            if (items == null || items.Count == 0)
                throw new ArgumentException("Order items cannot be empty.", nameof(items));

            decimal totalOrderPrice = 0m;

            // Create order shell
            var order = new Order
            {
                UID = uid,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 0m,
                Status = OrderStatus.Pending
            };
            AddOrder(order);

            foreach (var item in items)
            {
                var product = _productService.GetProductByName(item.ProductName)
                              ?? throw new Exception($"{item.ProductName} not found");

                if (product.StockQuantity < item.Quantity)
                    throw new Exception($"{product.ProductName} is out of stock");

                product.StockQuantity -= item.Quantity;

                var orderProducts = new OrderProducts
                {
                    OID = order.OID,
                    PID = product.PID,
                    Quantity = item.Quantity
                };

                _orderProductsService.AddOrderProducts(orderProducts);

                try
                {
                    _ctx.Products.Update(product);
                    _ctx.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw new InvalidOperationException(
                        $"Concurrency conflict: {product.ProductName} was updated by another user. Please try again.");
                }

                totalOrderPrice += item.Quantity * product.Price;
            }

            order.TotalAmount = totalOrderPrice;
            UpdateOrder(order);

            // Send email after success
            var user = _ctx.Users.FirstOrDefault(u => u.UID == uid);
            if (user != null)
            {
 
                    await _emailService.SendOrderPlacedEmail(user.Email, order.OID, totalOrderPrice);
            }
        }


        public async Task<OrderSummaryDTO?> GetOrderDetails(int orderId)
        {
            var order = _ctx.Orders
                .Include(o => o.OrderProducts).ThenInclude(i => i.product)
                .Include(o => o.user)
                .FirstOrDefault(o => o.OID == orderId);

            if (order is null) return null;

            return new OrderSummaryDTO
            {
                OrderId = order.OID,
                CustomerName = order.user.UName,
                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                Lines = order.OrderProducts.Select(i => new OrderLineDTO
                {
                    ProductName = i.product.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.product.Price
                }).ToList()
            };
        }
        public async Task CancelOrder(int orderId, int uid)
        {
            var order = _ctx.Orders.FirstOrDefault(o => o.OID == orderId && o.UID == uid);
            if (order == null)
                throw new Exception("Order not found");

            order.Status = OrderStatus.Cancelled;
            _ctx.Orders.Update(order);
            await _ctx.SaveChangesAsync();

            var user = _ctx.Users.FirstOrDefault(u => u.UID == uid);
            if (user != null)
            {
                    await _emailService.SendOrderCancelledEmail(user.Email, orderId);

            }
        }
        public OrderSummaryDTO GetOrderSummary(int orderId)
        {
            var order = _ctx.Orders
                .Include(o => o.user)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.product)
                .FirstOrDefault(o => o.OID == orderId);

            if (order == null)
                return null;

            return new OrderSummaryDTO
            {
                OrderId = order.OID,
                CustomerName = order.user.UName,  
                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,  
                Lines = order.OrderProducts.Select(op => new OrderLineDTO
                {
                    ProductName = op.product.ProductName,
                    Quantity = op.Quantity,
                    UnitPrice = op.product.Price
                }).ToList()
            };
        }
    }
}
