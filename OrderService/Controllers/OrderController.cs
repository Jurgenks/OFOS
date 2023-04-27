using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OFOS.Domain.Models;
using OrderService.Core;
using System.Security.Claims;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrderAsync([FromBody] OrderRequest orderRequest)
        {
            try
            {
                var products = orderRequest.Products;

                if (products == null || !products.Any())
                {
                    return BadRequest("The products list cannot be null or empty.");
                }

                foreach (var product in products)
                {
                    if (string.IsNullOrWhiteSpace(product.Name))
                    {
                        return BadRequest("A product name cannot be null or empty.");
                    }

                    if (product.Price <= 0)
                    {
                        return BadRequest("A product price must be greater than 0.");
                    }

                    if (product.Stock < 1)
                    {
                        return BadRequest("A product stock must be 1 or greater.");
                    }
                }

                var restaurantId = orderRequest.RestaurantId;

                if(restaurantId == Guid.Empty)
                {
                    return BadRequest("No restaurant specified.");
                }

                //Create the order and retrieve the OrderNumber
                var orderNumber = await _orderService.CreateOrderAsync(User, restaurantId, products);

                if(orderNumber == null)
                {
                    return StatusCode(500,"Failed to create order");
                }


                return Ok(new { orderNumber });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderAsync([FromQuery] Guid orderId)
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

        [HttpGet("order")]
        public async Task<IActionResult> GetOrderByOrderNumberAsync([FromQuery] string orderNumber)
        {
            try
            {
                var order = await _orderService.GetOrderByOrderNumberAsync(orderNumber);
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

        [HttpGet("user")]
        public async Task<IActionResult> GetOrdersForUserAsync([FromQuery] Guid userId)
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

        [HttpGet("restaurant")]
        public async Task<IActionResult> GetOrdersForRestaurantAsync([FromQuery] Guid restaurantId)
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

        public class OrderRequest
        {
            public Guid RestaurantId { get; set; }
            public List<Product> Products { get; set; }
        }
    }
}
