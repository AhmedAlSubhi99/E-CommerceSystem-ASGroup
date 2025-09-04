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
                .Include(o => o.OrderProducts).ThenInclude(op => op.Product)
                .Include(o => o.User)
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
                .Include(o => o.OrderProducts).ThenInclude(op => op.Product)
                .Include(o => o.User)
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
                .Include(o => o.OrderProducts).ThenInclude(op => op.Product)
                .Include(o => o.User)
                .FirstOrDefault(o => o.OID == orderId);

            if (order == null)
            {
                _logger.LogWarning("GetSummaryByOrderId failed. OrderId={OrderId} not found.", orderId);
                throw new KeyNotFoundException($"Order {orderId} not found.");
            }

            _logger.LogInformation("Fetched summary by OrderId={OrderId}", orderId);
            return _mapper.Map<OrderSummaryDTO>(order);
        }

        public async Task<IEnumerable<OrderSummaryDTO>> GetSummariesAsync(int pageNumber = 1, int pageSize = 20)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var orders = await _orders.GetAllOrdersAsync();

            var pagedOrders = orders
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var summaries = pagedOrders.Select(o => _mapper.Map<OrderSummaryDTO>(o)).ToList();

            _logger.LogInformation("Fetched {Count} orders for page {Page}, size {PageSize}",
                summaries.Count, pageNumber, pageSize);

            return summaries;
        }

        public async Task<List<OrderSummaryDTO>> GetUserOrderSummariesAsync(int userId)
        {
            var result = await _ctx.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ProjectTo<OrderSummaryDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            _logger.LogInformation("Fetched {Count} order summaries for UserId={UserId}", result.Count, userId);
            return result;
        }
    }
}
