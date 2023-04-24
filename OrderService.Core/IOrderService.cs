using OFOS.Domain.Models;

namespace OrderService.Core
{
    public interface IOrderService
    {
        Task<string> CreateOrderAsync(Guid userId, Guid restaurantId, List<Product> products);
        Task<Order?> GetOrderAsync(Guid orderId);
        Task<List<Order>?> GetOrdersForUserAsync(Guid userId);
        Task<List<Order>?> GetOrdersForRestaurantAsync(Guid restaurantId);
        Task<Order> UpdateOrderStatusAsync(Guid orderId, string newStatus);
        Task<Order> GetOrderByOrderNumberAsync(string orderNumber);
    }

}
