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

        public OrderSummaryService(IOrderService orders,
                                   IOrderProductsService orderProducts,
                                   IProductService products,
                                   IUserService users,
                                   ApplicationDbContext ctx,
                                   IMapper mapper)
        {
            _orders = orders;
            _orderProducts = orderProducts;
            _products = products;
            _users = users;
            _ctx = ctx;
            _mapper = mapper;
        }

        // --------------------------
        // Get summary for single order
        // --------------------------
        public OrderSummaryDTO? GetOrderSummary(int orderId)
        {
            var order = _ctx.Orders
                .Include(o => o.OrderProducts).ThenInclude(op => op.product)
                .Include(o => o.user)
                .FirstOrDefault(o => o.OID == orderId);

            if (order == null) return null;

            return _mapper.Map<OrderSummaryDTO>(order);
        }

        public async Task<OrderSummaryDTO?> GetOrderSummaryAsync(int orderId)
        {
            var order = await _ctx.Orders
                .Include(o => o.OrderProducts).ThenInclude(op => op.product)
                .Include(o => o.user)
                .FirstOrDefaultAsync(o => o.OID == orderId);

            if (order == null) return null;

            return _mapper.Map<OrderSummaryDTO>(order);
        }

        // --------------------------
        // Get summary by orderId (throws if not found)
        // --------------------------
        public OrderSummaryDTO GetSummaryByOrderId(int orderId)
        {
            var order = _ctx.Orders
                .Include(o => o.OrderProducts).ThenInclude(op => op.product)
                .Include(o => o.user)
                .FirstOrDefault(o => o.OID == orderId);

            if (order == null)
                throw new KeyNotFoundException($"Order {orderId} not found.");

            return _mapper.Map<OrderSummaryDTO>(order);
        }

        // --------------------------
        // Get paginated summaries
        // --------------------------
        public IEnumerable<OrderSummaryDTO> GetSummaries(int pageNumber = 1, int pageSize = 20)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            return _orders.GetAllOrders()
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(o => GetSummaryByOrderId(o.OID))
                .ToList();
        }

        // --------------------------
        // Get all user orders (async)
        // --------------------------
        public async Task<List<OrderSummaryDTO>> GetUserOrderSummariesAsync(int userId)
        {
            return await _ctx.Orders
                .AsNoTracking()
                .Where(o => o.UID == userId)
                .OrderByDescending(o => o.OrderDate)
                .ProjectTo<OrderSummaryDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        // --------------------------
        // Get summary with date filter (Admin dashboard)
        // --------------------------
        public AdminOrderSummaryDTO GetSummary(DateTime? from = null, DateTime? to = null)
        {
            var query = _ctx.Orders
                .Include(o => o.OrderProducts).ThenInclude(i => i.product)
                .Include(o => o.user)
                .AsQueryable();

            if (from != null) query = query.Where(o => o.OrderDate >= from);
            if (to != null) query = query.Where(o => o.OrderDate <= to);

            var orders = query.ToList();

            return new AdminOrderSummaryDTO
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
        }
    }
}
