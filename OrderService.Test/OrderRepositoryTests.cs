using Microsoft.EntityFrameworkCore;
using OFOS.Domain.Models;
using OrderService.Core;
using OrderService.Data;

namespace OrderService.Test
{
    [TestClass]
    public class OrderRepositoryTests
    {
        private OrderDbContext _dbContext;
        private IOrderRepository _orderRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            var options = new DbContextOptionsBuilder<OrderDbContext>()
                .UseInMemoryDatabase("testDatabase")
                .Options;
            _dbContext = new OrderDbContext(options);
            _orderRepository = new OrderRepository(_dbContext);
        }

        [TestMethod]
        public async Task CreateOrderAsync_ShouldCreateOrder()
        {
            // Arrange
            var order = new Order
            {
                UserId = Guid.NewGuid(),
                RestaurantId = Guid.NewGuid(),
                OrderNumber = "123",
                TotalPrice = 10m,
                Status = "Created",
                UpdatedAt = DateTime.UtcNow
            };

            // Act
            await _orderRepository.CreateOrderAsync(order);

            // Assert
            Assert.AreEqual(1, await _dbContext.Orders.CountAsync());
            var createdOrder = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == order.Id);
            Assert.IsNotNull(createdOrder);
            Assert.AreEqual(order.UserId, createdOrder.UserId);
            Assert.AreEqual(order.RestaurantId, createdOrder.RestaurantId);
            Assert.AreEqual(order.OrderNumber, createdOrder.OrderNumber);
            Assert.AreEqual(order.TotalPrice, createdOrder.TotalPrice);
            Assert.AreEqual(order.Status, createdOrder.Status);
            Assert.AreEqual(order.Products, createdOrder.Products);
            Assert.AreEqual(order.CreatedAt, createdOrder.CreatedAt);
            Assert.AreEqual(order.UpdatedAt, createdOrder.UpdatedAt);
        }

        [TestMethod]
        public async Task GetOrderAsync_ShouldReturnCorrectOrder()
        {
            // Arrange
            var order = new Order();
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _orderRepository.GetOrderAsync(order.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(order.Id, result.Id);
        }

        [TestMethod]
        public async Task GetOrdersForRestaurantAsync_ShouldReturnOrdersForRestaurant()
        {
            // Arrange
            var restaurantId = Guid.NewGuid();
            var orders = new List<Order>
        {
            new Order { RestaurantId = restaurantId },
            new Order { RestaurantId = Guid.NewGuid() },
            new Order { RestaurantId = restaurantId }
        };
            _dbContext.Orders.AddRange(orders);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _orderRepository.GetOrdersForRestaurantAsync(restaurantId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(o => o.RestaurantId == restaurantId));
        }

        [TestMethod]
        public async Task GetOrdersForUserAsync_ShouldReturnOrdersForUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orders = new List<Order>
        {
            new Order { UserId = userId },
            new Order { UserId = Guid.NewGuid() },
            new Order { UserId = userId }
        };
            _dbContext.Orders.AddRange(orders);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _orderRepository.GetOrdersForUserAsync(userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(o => o.UserId == userId));
        }

        [TestMethod]
        public async Task UpdateOrderAsync_ShouldUpdateOrderStatus()
        {
            // Arrange
            var order = new Order { Status = "Created" };
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // Act
            order.Status = "Paid";
            await _orderRepository.UpdateOrderAsync(order);
            var result = await _dbContext.Orders.FindAsync(order.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Paid", result.Status);
        }
    }

}
