using AutoMapper;
using E_CommerceSystem.Models.DTO;
using E_CommerceSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace E_CommerceSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IInvoiceService _invoiceService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            IInvoiceService invoiceService,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _invoiceService = invoiceService;
            _logger = logger;
        }

        // ---------------------------
        // Place Order
        // ---------------------------
        [HttpPost("place")]
        public async Task<IActionResult> PlaceOrder([FromBody] List<OrderItemDTO> items)
        {
            int userId = GetUserId();
            var order = await _orderService.PlaceOrderAsync(items, userId);
            return Ok(order);
        }

        // ---------------------------
        // Get My Orders
        // ---------------------------
        [HttpGet("my")]
        public async Task<IActionResult> GetMyOrders()
        {
            int userId = GetUserId();
            var orders = await _orderService.GetOrdersByUserAsync(userId);
            return Ok(orders);
        }

        // ---------------------------
        // Get All Orders (Admin/Manager)
        // ---------------------------
        [Authorize(Roles = "admin,manager")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var orders = await _orderService.GetAllOrdersAsync(page, pageSize);
            return Ok(orders);
        }

        // ---------------------------
        // Get Order Details
        // ---------------------------
        [HttpGet("{orderId:int}")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            var order = await _orderService.GetOrderDetailsAsync(orderId);
            return order == null ? NotFound() : Ok(order);
        }

        // ---------------------------
        // Update Order Status (Admin/Manager or Owner Cancel)
        // ---------------------------
        [HttpPatch("{orderId:int}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.UStatus))
                return BadRequest("Status is required.");

            int userId = GetUserId();
            bool isAdminOrManager = User.IsInRole("admin") || User.IsInRole("manager");

            try
            {
                var updated = await _orderService.UpdateOrderStatusAsync(orderId, dto, userId, isAdminOrManager);
                return updated != null ? Ok(updated) : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update status for Order {OrderId}", orderId);
                return BadRequest(ex.Message);
            }
        }
        // Cancel Order
        [HttpPost("{orderId:int}/cancel")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            int userId = GetUserId();
            var cancelled = await _orderService.CancelOrderAsync(orderId, userId);

            if (cancelled)
            {
                _logger.LogInformation("User {UserId} cancelled order {OrderId}", userId, orderId);
                return Ok(new { message = "Order cancelled successfully" });
            }

            return BadRequest("Unable to cancel order.");
        }

        // Invoice
        [HttpGet("{orderId:int}/invoice")]
        public async Task<IActionResult> DownloadInvoice(int orderId)
        {
            int userId = GetUserId();
            bool isAdmin = User.IsInRole("Admin"); // ensure consistent case

            var result = await _invoiceService.GeneratePdfAsync(orderId, userId, isAdmin);
            if (result == null)
            {
                _logger.LogWarning("Invoice generation failed for Order {OrderId} by User {UserId}", orderId, userId);
                return NotFound("Invoice not available or access denied.");
            }

            var (bytes, fileName) = result.Value;
            _logger.LogInformation("Invoice generated for Order {OrderId} by User {UserId}", orderId, userId);
            return File(bytes, "application/pdf", fileName);
        }

        // ---------------------------
        // Helpers
        // ---------------------------
        private int GetUserId()
        {
            var subClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            if (subClaim == null || !int.TryParse(subClaim.Value, out var userId))
                throw new UnauthorizedAccessException("Invalid user id in token.");
            return userId;
        }
    }
}
