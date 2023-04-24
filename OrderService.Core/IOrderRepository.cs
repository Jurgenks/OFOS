using OFOS.Domain.Models;

namespace OrderService.Core
{
    public interface IOrderRepository
    {
        Task CreateOrderAsync(Order order);
        Task<Order?> GetOrderAsync(Guid orderId);
        Task<Order?> GetOrderByOrderNumberAsync(string orderNumber);
        Task<List<Order>?> GetOrdersForRestaurantAsync(Guid restaurantId);
        Task<List<Order>?> GetOrdersForUserAsync(Guid userId);
        Task UpdateOrderAsync(Order order);
        Task DeleteOrderAsync(Order order);
        Task<int> GetOrdersCountAsync();
    }

}
