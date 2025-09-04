using E_CommerceSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace E_CommerceSystem.Services
{
    public class ReportsService : IReportsService
    {
        private readonly ApplicationDbContext _ctx;
        private readonly ILogger<ReportsService> _logger;

        public ReportsService(ApplicationDbContext ctx, ILogger<ReportsService> logger)
        {
            _ctx = ctx;
            _logger = logger;
        }

        // Treat Paid/Shipped/Delivered as sales; exclude Cancelled/Pending
        private static bool IsCompletedStatus(OrderStatus s) =>
            s == OrderStatus.Paid || s == OrderStatus.Shipped || s == OrderStatus.Delivered;

        public async Task<IReadOnlyList<BestSellingProductDTO>> GetBestSellingProductsAsync(DateTime? from, DateTime? to, int take = 10)
        {
            _logger.LogInformation("Fetching best-selling products (Top {Take}) from {From} to {To}.", take, from, to);

            var qOrders = _ctx.Orders.AsNoTracking().Where(o => IsCompletedStatus(o.Status));
            if (from.HasValue) qOrders = qOrders.Where(o => o.OrderDate >= from.Value);
            if (to.HasValue) qOrders = qOrders.Where(o => o.OrderDate < to.Value);

            var q = from op in _ctx.OrderProducts.AsNoTracking()
                    join o in qOrders on op.OID equals o.OID
                    join p in _ctx.Products.AsNoTracking() on op.PID equals p.PID
                    group new { op, p } by new { p.PID, p.ProductName, p.Price } into g
                    orderby g.Sum(x => x.op.Quantity) descending
                    select new BestSellingProductDTO
                    {
                        ProductId = g.Key.PID,
                        ProductName = g.Key.ProductName,
                        UnitsSold = g.Sum(x => x.op.Quantity),
                        Revenue = g.Sum(x => x.op.Quantity * x.p.Price)
                    };

            var result = await q.Take(take).ToListAsync();
            _logger.LogInformation("Best-selling products report generated with {Count} results.", result.Count);
            return result;
        }

        public async Task<IReadOnlyList<RevenueByDayDTO>> GetRevenueByDayAsync(DateTime from, DateTime to)
        {
            _logger.LogInformation("Fetching revenue by day from {From} to {To}.", from, to);

            var q = _ctx.Orders.AsNoTracking()
                .Where(o => IsCompletedStatus(o.Status) && o.OrderDate >= from && o.OrderDate < to)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new RevenueByDayDTO { Date = g.Key, Revenue = g.Sum(x => x.TotalAmount) })
                .OrderBy(x => x.Date);

            var result = await q.ToListAsync();
            _logger.LogInformation("Revenue by day report generated with {Count} days.", result.Count);
            return result;
        }

        public async Task<IReadOnlyList<RevenueByMonthDTO>> GetRevenueByMonthAsync(int year)
        {
            _logger.LogInformation("Fetching revenue by month for year {Year}.", year);

            var q = _ctx.Orders.AsNoTracking()
                .Where(o => IsCompletedStatus(o.Status) && o.OrderDate.Year == year)
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new RevenueByMonthDTO
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(x => x.TotalAmount)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month);

            var result = await q.ToListAsync();
            _logger.LogInformation("Revenue by month report generated with {Count} months.", result.Count);
            return result;
        }

        public async Task<IReadOnlyList<TopRatedProductDTO>> GetTopRatedProductsAsync(int minReviews = 3, int take = 10)
        {
            _logger.LogInformation("Fetching top-rated products (MinReviews={MinReviews}, Take={Take}).", minReviews, take);

            var q = _ctx.Reviews.AsNoTracking()
                .GroupBy(r => new { r.PID, r.product.ProductName })
                .Select(g => new TopRatedProductDTO
                {
                    ProductId = g.Key.PID,
                    ProductName = g.Key.ProductName,
                    ReviewsCount = g.Count(),
                    Rating = g.Average(x => x.Rating)
                })
                .Where(x => x.ReviewsCount >= minReviews)
                .OrderByDescending(x => x.Rating)
                .ThenByDescending(x => x.ReviewsCount);

            var result = await q.Take(take).ToListAsync();
            _logger.LogInformation("Top-rated products report generated with {Count} results.", result.Count);
            return result;
        }

        public async Task<IReadOnlyList<MostActiveCustomerDTO>> GetMostActiveCustomersAsync(DateTime? from, DateTime? to, int take = 10)
        {
            _logger.LogInformation("Fetching most active customers (Top {Take}) from {From} to {To}.", take, from, to);

            var qOrders = _ctx.Orders.AsNoTracking().Where(o => IsCompletedStatus(o.Status));
            if (from.HasValue) qOrders = qOrders.Where(o => o.OrderDate >= from.Value);
            if (to.HasValue) qOrders = qOrders.Where(o => o.OrderDate < to.Value);

            var q = from g in qOrders.GroupBy(o => o.UID)
                    join u in _ctx.Users.AsNoTracking() on g.Key equals u.UID
                    orderby g.Count() descending, g.Sum(x => x.TotalAmount) descending
                    select new MostActiveCustomerDTO
                    {
                        UserId = u.UID,
                        UserName = u.UName,
                        OrdersCount = g.Count(),
                        TotalSpent = g.Sum(x => x.TotalAmount)
                    };

            var result = await q.Take(take).ToListAsync();
            _logger.LogInformation("Most active customers report generated with {Count} results.", result.Count);
            return result;
        }
    }
}
