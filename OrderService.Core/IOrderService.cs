using OFOS.Domain.Models;
using System.Security.Claims;

namespace OrderService.Core
{
    public interface IOrderService
    {
        Task<string> CreateOrderAsync(ClaimsPrincipal user, Guid restaurantId, List<Product> products);
        Task<Order?> GetOrderAsync(Guid orderId);
        Task<List<Order>?> GetOrdersForUserAsync(Guid userId);
        Task<List<Order>?> GetOrdersForRestaurantAsync(Guid restaurantId);
        Task<Order> UpdateOrderStatusAsync(Guid orderId, string newStatus);
        Task<Order> GetOrderByOrderNumberAsync(string orderNumber);
    }

}
