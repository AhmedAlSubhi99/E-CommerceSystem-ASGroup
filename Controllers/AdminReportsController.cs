using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_CommerceSystem.Controllers
{
    [Authorize(Roles = "admin,manager")]
    [ApiController]
    [Route("api/admin/reports")]
    public class AdminReportsController : ControllerBase
    {
        private readonly IReportsService _reports;

        public AdminReportsController(IReportsService reports) => _reports = reports;

        // 1) Best-selling products
        [HttpGet("best-selling")]
        public async Task<ActionResult<IReadOnlyList<BestSellingProductDTO>>> GetBestSelling(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int take = 10)
            => Ok(await _reports.GetBestSellingProductsAsync(from, to, take));

        // 2) Revenue by day
        [HttpGet("revenue/daily")]
        public async Task<ActionResult<IReadOnlyList<RevenueByDayDTO>>> GetRevenueDaily(
            [FromQuery] DateTime from, [FromQuery] DateTime to)
            => Ok(await _reports.GetRevenueByDayAsync(from, to));

        // 3) Revenue by month
        [HttpGet("revenue/monthly")]
        public async Task<ActionResult<IReadOnlyList<RevenueByMonthDTO>>> GetRevenueMonthly(
            [FromQuery] int year)
            => Ok(await _reports.GetRevenueByMonthAsync(year));

        // 4) Top-rated products
        [HttpGet("top-rated")]
        public async Task<ActionResult<IReadOnlyList<TopRatedProductDTO>>> GetTopRated(
            [FromQuery] int minReviews = 3, [FromQuery] int take = 10)
            => Ok(await _reports.GetTopRatedProductsAsync(minReviews, take));

        // 5) Most active customers
        [HttpGet("most-active")]
        public async Task<ActionResult<IReadOnlyList<MostActiveCustomerDTO>>> GetMostActive(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int take = 10)
            => Ok(await _reports.GetMostActiveCustomersAsync(from, to, take));
    }
}
