using OFOS.Domain.Models;
using OrderService.Data;

namespace OrderService.Core
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _dbContext;

        public OrderRepository(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// Creates a new order in the database
        public async Task CreateOrderAsync(Order order)
        {
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();
        }

        /// Gets an order by its ID
        public async Task<Order?> GetOrderAsync(Guid orderId)
        {
            return await Task.FromResult(_dbContext.Orders.FirstOrDefault(i => i.Id.Equals(orderId)));
        }

        /// Gets all orders for a given restaurant
        public async Task<List<Order>?> GetOrdersForRestaurantAsync(Guid restaurantId)
        {
            var orders = _dbContext.Orders.Where(i => i.RestaurantId.Equals(restaurantId));

            if (orders.Any())
            {
                return await Task.FromResult(orders.ToList());
            }
            else
            {
                return null;
            }
        }

        /// Gets all orders for a given user
        public async Task<List<Order>?> GetOrdersForUserAsync(Guid userId)
        {
            var orders = _dbContext.Orders.Where(i => i.UserId.Equals(userId));

            if (orders.Any())
            {
                return await Task.FromResult(orders.ToList());
            }
            else
            {
                return null;
            }
        }

        /// Updates the status of an order
        public async Task UpdateOrderAsync(Order order)
        {
            _dbContext.Orders.Update(order);
            await _dbContext.SaveChangesAsync();
        }

        /// Deletes the order from the database
        public async Task DeleteOrderAsync(Order order)
        {
            _dbContext.Orders.Remove(order);
            await _dbContext.SaveChangesAsync();
        }
    }

}
