using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
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
        private readonly IOrderSummaryService _orderSummaryService;
        private readonly IInvoiceService _invoiceService;
        private readonly ILogger<OrderController> _logger;
        private readonly IMapper _mapper;

        public OrderController(
            IOrderService orderService,
            IOrderSummaryService orderSummaryService,
            IInvoiceService invoiceService,
            ILogger<OrderController> logger,
            IMapper mapper)
        {
            _orderService = orderService;
            _orderSummaryService = orderSummaryService;
            _invoiceService = invoiceService;
            _logger = logger;
            _mapper = mapper;
        }

        // ---------------------------
        // Place Order
        // ---------------------------
        [HttpPost("PlaceOrder")]
        public async Task<IActionResult> PlaceOrder([FromBody] List<OrderItemDTO> items, int uid)
        {
            await _orderService.PlaceOrder(items, uid);
            return Ok("Order placed successfully.");
        }

        // ---------------------------
        // Get All Orders (for current user)
        // ---------------------------
        [HttpGet("GetAllOrders")]
        public IActionResult GetAllOrders()
        {
            int uid = GetUserId();
            var entities = _orderService.GetAllOrders(uid);
            var dtos = _mapper.Map<List<OrdersOutputDTO>>(entities);
            return Ok(dtos);
        }

        // ---------------------------
        // Get Order By Id
        // ---------------------------
        [HttpGet("GetOrderById/{orderId:int}")]
        public IActionResult GetOrderById(int orderId)
        {
            int uid = GetUserId();
            var rows = _orderService.GetOrderById(orderId, uid);
            return rows == null ? NotFound() : Ok(rows);
        }

        // ---------------------------
        // Get Order Summary
        // ---------------------------
        [HttpGet("{orderId:int}/summary")]
        public ActionResult<OrderSummaryDTO> GetSummary(int orderId) =>
            Ok(_orderSummaryService.GetSummaryByOrderId(orderId));

        // ---------------------------
        // Get Paged Summaries (Admin only)
        // ---------------------------
        [Authorize(Roles = "admin")]
        [HttpGet("summaries")]
        public ActionResult<IEnumerable<OrderSummaryDTO>> GetSummaries(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20) =>
            Ok(_orderSummaryService.GetSummaries(pageNumber, pageSize));

        // ---------------------------
        // Cancel Order
        // ---------------------------
        [HttpPost("CancelOrder/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId, int uid)
        {
            await _orderService.CancelOrder(orderId, uid);
            return Ok("Order cancelled successfully.");
        }

        // ---------------------------
        // Update Order Status (Admin/Manager)
        // ---------------------------
        [Authorize(Roles = "admin,manager")]
        [HttpPatch("{orderId:int}/status")]
        public IActionResult UpdateStatus(int orderId, [FromBody] UpdateOrderStatusDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Status))
                return BadRequest("Status is required.");

            int userId = GetUserId();
            var newStatus = Enum.Parse<OrderStatus>(dto.Status, true);

            var updated = _orderService.SetStatus(orderId, newStatus, userId, true);
            return updated != null
                ? Ok(_mapper.Map<OrdersOutputDTO>(updated))
                : BadRequest("Invalid status update.");
        }

        // ---------------------------
        // Quick status transitions
        // ---------------------------
        [Authorize(Roles = "admin,manager")]
        [HttpPost("{orderId:int}/Pay")]
        public IActionResult MarkPaid(int orderId)
        {
            int userId = GetUserId();
            var updated = _orderService.SetStatus(orderId, OrderStatus.Paid, userId, true);
            return updated != null
                ? Ok(_mapper.Map<OrdersOutputDTO>(updated))
                : BadRequest("Unable to mark as paid.");
        }

        [Authorize(Roles = "admin,manager")]
        [HttpPost("{orderId:int}/Ship")]
        public IActionResult MarkShipped(int orderId)
        {
            int userId = GetUserId();
            var updated = _orderService.SetStatus(orderId, OrderStatus.Paid, userId, true);
            return updated != null
                ? Ok(_mapper.Map<OrdersOutputDTO>(updated))
                : BadRequest("Unable to mark as Ship.");
        }

        [Authorize(Roles = "admin,manager")]
        [HttpPost("{orderId:int}/Deliver")]
        public IActionResult MarkDelivered(int orderId)
        {
            int userId = GetUserId();
            var updated = _orderService.SetStatus(orderId, OrderStatus.Paid, userId, true);
            return updated != null
                ? Ok(_mapper.Map<OrdersOutputDTO>(updated))
                : BadRequest("Unable to mark as Deliver.");
        }

        // ---------------------------
        // Order Details
        // ---------------------------
        [HttpGet("{orderId:int}/details")]
        public ActionResult<OrderSummaryDTO> GetOrderDetails(int orderId)
        {
            var details = _orderService.GetOrderDetails(orderId);
            return details is null ? NotFound() : Ok(details);
        }

        // ---------------------------
        // Admin Aggregated Summary
        // ---------------------------
        [Authorize(Roles = "admin")]
        [HttpGet("summary")]
        public ActionResult<AdminOrderSummaryDTO> GetSummary(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to) =>
            Ok(_orderSummaryService.GetSummary(from, to));
        // ---------------------------
        // Invoice (sync)
        // ---------------------------
        [HttpGet("{orderId:int}/invoice-sync")]
        public IActionResult GetInvoice(int orderId)
        {
            int userId = GetUserId();
            bool isAdmin = User.IsInRole("admin");

            var pdf = _invoiceService.GenerateInvoice(orderId, userId, isAdmin);
            if (pdf == null) return NotFound("Invoice not found or access denied.");

            return File(pdf, "application/pdf", $"Invoice_{orderId}.pdf");
        }

        // ---------------------------
        // Invoice (async)
        // ---------------------------
        [HttpGet("{orderId:int}/invoice")]
        public async Task<IActionResult> DownloadInvoice(
            int orderId,
            [FromServices] IInvoiceService invoiceService)
        {
            int requestUserId = GetUserId();

            var result = await invoiceService.GeneratePdfAsync(orderId, requestUserId);
            if (result == null)
                return NotFound("Invoice not available or you don't have access to this order.");

            var (bytes, fileName) = result.Value;
            return File(bytes, "application/pdf", fileName);
        }


        [Authorize(Roles = "admin,manager")]
        [HttpPut("UpdateStatus/{orderId}")]
        public IActionResult UpdateOrderStatus(int orderId, [FromBody] OrderStatus newStatus)
        {
            try
            {
                _orderService.UpdateOrderStatus(orderId, newStatus);
                return Ok($"Order {orderId} status updated to {newStatus}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update status for Order {OrderId}", orderId);
                return BadRequest(ex.Message);
            }
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
