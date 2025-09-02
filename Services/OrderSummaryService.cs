using AutoMapper;
using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;


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
                                   IUserService users, ApplicationDbContext ctx, IMapper mapper)
        {
            _orders = orders;
            _orderProducts = orderProducts;
            _products = products;
            _users = users;
            _ctx = ctx;
            _mapper = mapper;
        }

        public OrderSummaryDTO GetSummaryByOrderId(int orderId)
        {
            var order = _orders.GetOrderEntityById(orderId)
                       ?? throw new KeyNotFoundException($"Order {orderId} not found.");

            var lines = _orderProducts.GetOrdersByOrderId(orderId);
            var lineDtos = new List<OrderLineDTO>();
            decimal subtotal = 0m;

            foreach (var l in lines)
            {
                var p = _products.GetProductById(l.PID)
                        ?? throw new KeyNotFoundException($"Product {l.PID} not found.");

                var dto = new OrderLineDTO
                {
                    ProductId = p.PID,
                    ProductName = p.ProductName,
                    Quantity = l.Quantity,
                    UnitPrice = p.Price
                };
                subtotal += dto.LineTotal;
                lineDtos.Add(dto);
            }

            var total = subtotal;

            return new OrderSummaryDTO
            {
                OrderId = order.OID,
                CustomerName = _users.GetUserById(order.UID)?.UName,
                CreatedAt = order.OrderDate,
                Status = order.Status ?? "Pending",
                Lines = lineDtos,
                Subtotal = subtotal,
                Total = total
            };
        }

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
        public async Task<List<OrderSummaryDTO>> GetUserOrderSummariesAsync(int userId)
        {
            return await _ctx.Orders
                .AsNoTracking()
                .Where(o => o.UID == userId)
                .OrderByDescending(o => o.OrderDate)
                // ensure navigations are available to the mapper
                .ProjectTo<OrderSummaryDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<OrderSummaryDTO?> GetOrderSummaryAsync(int orderId, int userId)
        {
            return await _ctx.Orders
                .AsNoTracking()
                .Where(o => o.OID == orderId && o.UID == userId)
                .ProjectTo<OrderSummaryDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }
    }
}
