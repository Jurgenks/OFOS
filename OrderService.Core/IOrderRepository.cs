using OFOS.Domain.Models;

namespace OrderService.Core
{
    public interface IOrderRepository
    {
        Task CreateOrderAsync(Order order);
        Task<Order?> GetOrderAsync(Guid orderId);
        Task<List<Order>?> GetOrdersForRestaurantAsync(Guid restaurantId);
        Task<List<Order>?> GetOrdersForUserAsync(Guid userId);
        Task UpdateOrderAsync(Order order);
        Task DeleteOrderAsync(Order order);
    }

}
