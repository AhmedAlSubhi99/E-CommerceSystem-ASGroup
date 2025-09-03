using AutoMapper;
using AutoMapper.QueryableExtensions;
using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Services
{
    public class OrderSummaryService : IOrderSummaryService
    {
        private readonly IOrderService _orders;
        private readonly IOrderProductsService _orderProducts;
        private readonly IProductService _products;
        private readonly IUserService _users;
        private readonly ApplicationDbContext _ctx;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderSummaryService> _logger;

        public OrderSummaryService(IOrderService orders,
                                   IOrderProductsService orderProducts,
                                   IProductService products,
                                   IUserService users,
                                   ApplicationDbContext ctx,
                                   IMapper mapper,
                                   ILogger<OrderSummaryService> logger)
        {
            _orders = orders;
            _orderProducts = orderProducts;
            _products = products;
            _users = users;
            _ctx = ctx;
            _mapper = mapper;
            _logger = logger;
        }

        public OrderSummaryDTO? GetOrderSummary(int orderId)
        {
            var order = _ctx.Orders
                .Include(o => o.OrderProducts).ThenInclude(op => op.product)
                .Include(o => o.user)
                .FirstOrDefault(o => o.OID == orderId);

            if (order == null)
            {
                _logger.LogWarning("Order summary requested but not found. OrderId={OrderId}", orderId);
                return null;
            }

            _logger.LogInformation("Fetched order summary for OrderId={OrderId}", orderId);
            return _mapper.Map<OrderSummaryDTO>(order);
        }

        public async Task<OrderSummaryDTO?> GetOrderSummaryAsync(int orderId)
        {
            var order = await _ctx.Orders
                .Include(o => o.OrderProducts).ThenInclude(op => op.product)
                .Include(o => o.user)
                .FirstOrDefaultAsync(o => o.OID == orderId);

            if (order == null)
            {
                _logger.LogWarning("Async order summary requested but not found. OrderId={OrderId}", orderId);
                return null;
            }

            _logger.LogInformation("Fetched async order summary for OrderId={OrderId}", orderId);
            return _mapper.Map<OrderSummaryDTO>(order);
        }

        public OrderSummaryDTO GetSummaryByOrderId(int orderId)
        {
            var order = _ctx.Orders
                .Include(o => o.OrderProducts).ThenInclude(op => op.product)
                .Include(o => o.user)
                .FirstOrDefault(o => o.OID == orderId);

            if (order == null)
            {
                _logger.LogWarning("GetSummaryByOrderId failed. OrderId={OrderId} not found.", orderId);
                throw new KeyNotFoundException($"Order {orderId} not found.");
            }

            _logger.LogInformation("Fetched summary by OrderId={OrderId}", orderId);
            return _mapper.Map<OrderSummaryDTO>(order);
        }

        public IEnumerable<OrderSummaryDTO> GetSummaries(int pageNumber = 1, int pageSize = 20)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var orders = _orders.GetAllOrders()
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var summaries = orders.Select(o => GetSummaryByOrderId(o.OID)).ToList();

            _logger.LogInformation("Fetched {Count} order summaries for page={Page}, pageSize={PageSize}.",
                                   summaries.Count, pageNumber, pageSize);

            return summaries;
        }

        public async Task<List<OrderSummaryDTO>> GetUserOrderSummariesAsync(int userId)
        {
            var result = await _ctx.Orders
                .AsNoTracking()
                .Where(o => o.UID == userId)
                .OrderByDescending(o => o.OrderDate)
                .ProjectTo<OrderSummaryDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            _logger.LogInformation("Fetched {Count} order summaries for UserId={UserId}", result.Count, userId);
            return result;
        }

        public AdminOrderSummaryDTO GetSummary(DateTime? from = null, DateTime? to = null)
        {
            var query = _ctx.Orders
                .Include(o => o.OrderProducts).ThenInclude(i => i.product)
                .Include(o => o.user)
                .AsQueryable();

            if (from != null) query = query.Where(o => o.OrderDate >= from);
            if (to != null) query = query.Where(o => o.OrderDate <= to);

            var orders = query.ToList();

            var result = new AdminOrderSummaryDTO
            {
                TotalOrders = orders.Count,
                TotalRevenue = orders.Sum(o => o.TotalAmount),
                TotalItemsSold = orders.Sum(o => o.OrderProducts.Sum(i => i.Quantity)),
                TopProducts = orders
                    .SelectMany(o => o.OrderProducts)
                    .GroupBy(i => new { i.PID, i.product.ProductName })
                    .Select(g => new ProductSummaryDTO
                    {
                        ProductId = g.Key.PID,
                        Name = g.Key.ProductName,
                        QuantitySold = g.Sum(i => i.Quantity)
                    })
                    .OrderByDescending(p => p.QuantitySold)
                    .Take(10)
                    .ToList(),
                TopCustomers = orders
                    .GroupBy(o => new { o.UID, o.user.UName })
                    .Select(g => new CustomerSummaryDTO
                    {
                        UserId = g.Key.UID,
                        Name = g.Key.UName,
                        TotalSpent = g.Sum(o => o.TotalAmount)
                    })
                    .OrderByDescending(c => c.TotalSpent)
                    .Take(10)
                    .ToList()
            };

            _logger.LogInformation("Admin order summary generated with {Orders} orders, {Revenue:C} total revenue, {Items} items sold.",
                                   result.TotalOrders, result.TotalRevenue, result.TotalItemsSold);

            return result;
        }
    }
}
