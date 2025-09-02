using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace E_CommerceSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[Controller]")]
    public class OrderController: ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("PlaceOrder")]
        public IActionResult PlaceOrder([FromBody] List<OrderItemDTO> items)
        {
            try
            {
                if (items == null || !items.Any())
                {
                    return BadRequest("Order items cannot be empty.");
                }

                // Retrieve the Authorization header from the request
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Decode the token to check user role
                var userId= GetUserIdFromToken(token);

                // Extract user ID 
                int uid = int.Parse(userId);

                _orderService.PlaceOrder(items, uid);

                return Ok("Order placed successfully.");
            }
            catch (Exception ex)
            {
                // Return a generic error response
                return StatusCode(500, $"An error occurred while placing order. {(ex.Message)}");

            }

        }
        [HttpGet("GetAllOrders")]
        public IActionResult GetAllOrders()
        {
            try
            {
                // Retrieve the Authorization header from the request
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Decode the token to check user role
                var userId = GetUserIdFromToken(token);

                // Extract user ID 
                int uid = int.Parse(userId);
                
                return Ok(_orderService.GetAllOrders(uid));
            }
            catch (Exception ex)
            {
                // Return a generic error response
                return StatusCode(500, $"An error occurred while retrieving products. {(ex.Message)}");

            }
        }

        [HttpGet("GetOrderById/{OrderId}")]
        public IActionResult GetOrderById(int OrderId)
        {
            try
            {
                // Retrieve the Authorization header from the request
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Decode the token to check user role
                var userId = GetUserIdFromToken(token);

                // Extract user ID 
                int uid = int.Parse(userId);

                return Ok(_orderService.GetOrderById(OrderId,uid));
            }
            catch (Exception ex)
            {
                // Return a generic error response
                return StatusCode(500, $"An error occurred while retrieving products. {(ex.Message)}");

            }
        }

        // Method to decode token to get user id
        private string? GetUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            if (handler.CanReadToken(token))
            {
                var jwtToken = handler.ReadJwtToken(token);

                // Extract the 'sub' claim
                var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub");


                return (subClaim?.Value); // Return both values as a tuple
            }

            throw new UnauthorizedAccessException("Invalid or unreadable token.");
        }
    }


}
