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
    [Route("api/[controller]")] // lower-case [controller] is fine
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;

        public OrderController(IOrderService orderService, IMapper mapper)
        {
            _orderService = orderService;
            _mapper = mapper;
        }

        [HttpPost("PlaceOrder")]
        public IActionResult PlaceOrder([FromBody] List<OrderItemDTO> items)
        {
            try
            {
                if (items == null || !items.Any())
                    return BadRequest("Order items cannot be empty.");

                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var userId = GetUserIdFromToken(token);
                int uid = int.Parse(userId);

                _orderService.PlaceOrder(items, uid);
                return Ok("Order placed successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while placing the order. ({ex.Message})");
            }
        }

        [HttpGet("GetAllOrders")]
        public IActionResult GetAllOrders()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var userId = GetUserIdFromToken(token);
                int uid = int.Parse(userId);

                var entities = _orderService.GetAllOrders(uid);
                var dtos = _mapper.Map<List<OrdersOutputDTO>>(entities);
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving orders. ({ex.Message})");
            }
        }

        [HttpGet("GetOrderById/{orderId:int}")]
        public IActionResult GetOrderById(int orderId)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var userId = GetUserIdFromToken(token);
                int uid = int.Parse(userId);

                var rows = _orderService.GetOrderById(orderId, uid); // returns IEnumerable<OrdersOutputDTO>
                return Ok(rows);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving orders. ({ex.Message})");
            }
        }

        private string? GetUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(token))
            {
                var jwtToken = handler.ReadJwtToken(token);
                var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub");
                return subClaim?.Value;
            }
            throw new UnauthorizedAccessException("Invalid or unreadable token.");
        }


        [Authorize]
        [HttpGet("{orderId}/summary")]
        public ActionResult<OrderSummaryDTO> GetSummary(
    int orderId,
    [FromServices] IOrderSummaryService summaryService)
        {
            return Ok(summaryService.GetSummaryByOrderId(orderId));
        }

        [Authorize(Roles = "admin")]
        [HttpGet("summaries")]
        public ActionResult<IEnumerable<OrderSummaryDTO>> GetSummaries(
    [FromServices] IOrderSummaryService summaryService,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 20)
            
        {
            return Ok(summaryService.GetSummaries(pageNumber, pageSize));
        }

    }
}
