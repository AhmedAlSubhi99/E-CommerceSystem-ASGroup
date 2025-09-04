using E_CommerceSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_CommerceSystem.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportsService _reportsService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IReportsService reportsService, ILogger<ReportController> logger)
        {
            _reportsService = reportsService;
            _logger = logger;
        }

        [HttpGet("best-selling-products")]
        public async Task<IActionResult> GetBestSellingProducts(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int take = 10)
        {
            _logger.LogInformation("User {User} requested Best-Selling Products report.", User.Identity?.Name);
            var result = await _reportsService.GetBestSellingProductsAsync(from, to, take);
            return Ok(result);
        }

        [HttpGet("revenue-by-day")]
        public async Task<IActionResult> GetRevenueByDay(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            _logger.LogInformation("User {User} requested Revenue by Day report from {From} to {To}.",
                                   User.Identity?.Name, from, to);

            var result = await _reportsService.GetRevenueByDayAsync(from, to);
            return Ok(result);
        }

        [HttpGet("revenue-by-month")]
        public async Task<IActionResult> GetRevenueByMonth([FromQuery] int year)
        {
            _logger.LogInformation("User {User} requested Revenue by Month report for year {Year}.",
                                   User.Identity?.Name, year);

            var result = await _reportsService.GetRevenueByMonthAsync(year);
            return Ok(result);
        }

        [HttpGet("top-rated-products")]
        public async Task<IActionResult> GetTopRatedProducts(
            [FromQuery] int minReviews = 3,
            [FromQuery] int take = 10)
        {
            _logger.LogInformation("User {User} requested Top-Rated Products report (MinReviews={MinReviews}, Take={Take}).",
                                   User.Identity?.Name, minReviews, take);

            var result = await _reportsService.GetTopRatedProductsAsync(minReviews, take);
            return Ok(result);
        }

        [HttpGet("most-active-customers")]
        public async Task<IActionResult> GetMostActiveCustomers(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int take = 10)
        {
            _logger.LogInformation("User {User} requested Most Active Customers report from {From} to {To} (Top {Take}).",
                                   User.Identity?.Name, from, to, take);

            var result = await _reportsService.GetMostActiveCustomersAsync(from, to, take);
            return Ok(result);
        }
    }
}
