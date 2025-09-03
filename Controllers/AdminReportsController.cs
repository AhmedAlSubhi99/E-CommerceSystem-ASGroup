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
        private readonly ILogger<AdminReportsController> _logger;

        public AdminReportsController(IReportsService reports, ILogger<AdminReportsController> logger)
        {
            _reports = reports;
            _logger = logger;
        }

        // 1) Best-selling products
        [HttpGet("best-selling")]
        public async Task<ActionResult<IReadOnlyList<BestSellingProductDTO>>> GetBestSelling(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int take = 10)
        {
            _logger.LogInformation("Admin requested best-selling products report (from={From}, to={To}, take={Take}).", from, to, take);
            return Ok(await _reports.GetBestSellingProductsAsync(from, to, take));
        }

        // 2) Revenue by day
        [HttpGet("revenue/daily")]
        public async Task<ActionResult<IReadOnlyList<RevenueByDayDTO>>> GetRevenueDaily(
            [FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            _logger.LogInformation("Admin requested daily revenue report (from={From}, to={To}).", from, to);
            return Ok(await _reports.GetRevenueByDayAsync(from, to));
        }

        // 3) Revenue by month
        [HttpGet("revenue/monthly")]
        public async Task<ActionResult<IReadOnlyList<RevenueByMonthDTO>>> GetRevenueMonthly([FromQuery] int year)
        {
            _logger.LogInformation("Admin requested monthly revenue report for year {Year}.", year);
            return Ok(await _reports.GetRevenueByMonthAsync(year));
        }

        // 4) Top-rated products
        [HttpGet("top-rated")]
        public async Task<ActionResult<IReadOnlyList<TopRatedProductDTO>>> GetTopRated(
            [FromQuery] int minReviews = 3, [FromQuery] int take = 10)
        {
            _logger.LogInformation("Admin requested top-rated products report (minReviews={MinReviews}, take={Take}).", minReviews, take);
            return Ok(await _reports.GetTopRatedProductsAsync(minReviews, take));
        }

        // 5) Most active customers
        [HttpGet("most-active")]
        public async Task<ActionResult<IReadOnlyList<MostActiveCustomerDTO>>> GetMostActive(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int take = 10)
        {
            _logger.LogInformation("Admin requested most active customers report (from={From}, to={To}, take={Take}).", from, to, take);
            return Ok(await _reports.GetMostActiveCustomersAsync(from, to, take));
        }
    }
}
