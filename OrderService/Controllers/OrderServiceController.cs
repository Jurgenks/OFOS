using Microsoft.AspNetCore.Mvc;
using OFOS.Domain.Models;
using OrderService.Core;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderServiceController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderServiceController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrderAsync(Guid userId, Guid restaurantId, List<Product> products)
        {
            try
            {
                await _orderService.CreateOrderAsync(userId, restaurantId, products);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderAsync(Guid orderId)
        {
            try
            {
                var order = await _orderService.GetOrderAsync(orderId);
                if (order == null)
                {
                    return NotFound();
                }
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetOrdersForUserAsync(Guid userId)
        {
            try
            {
                var orders = await _orderService.GetOrdersForUserAsync(userId);
                if (orders == null || orders.Count == 0)
                {
                    return NotFound();
                }
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("restaurant/{restaurantId}")]
        public async Task<IActionResult> GetOrdersForRestaurantAsync(Guid restaurantId)
        {
            try
            {
                var orders = await _orderService.GetOrdersForRestaurantAsync(restaurantId);
                if (orders == null || orders.Count == 0)
                {
                    return NotFound();
                }
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateOrderStatusAsync(Guid orderId, string newStatus)
        {
            try
            {
                var order = await _orderService.UpdateOrderStatusAsync(orderId, newStatus);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
