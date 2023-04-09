using Moq;
using OFOS.Domain.Models;
using OrderService.Core;
using Service = OrderService.Core.OrderService;

namespace OrderService.Test
{
    [TestClass]
    public class OrderServiceTests
    {
        private Mock<IOrderRepository>? _mockRepository;
        private Service? _orderService;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<IOrderRepository>();
            _orderService = new Service(_mockRepository.Object);
        }

        [TestMethod]
        public async Task CreateOrderAsync_ShouldCreateNewOrder()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var restaurantId = Guid.NewGuid();
            var products = new List<Product>
            {
            new Product("Product1","DescriptionText1",10m,100),
            new Product("Product2","DescriptionText2",20m,200),
            new Product("Product3","DescriptionText3",30m,300)
            };

            // Act
            await _orderService.CreateOrderAsync(userId, restaurantId, products);

            // Assert
            _mockRepository.Verify(r => r.CreateOrderAsync(It.IsAny<Order>()), Times.Once);
        }

        [TestMethod]
        public async Task GetOrderAsync_ShouldReturnOrder()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order(orderId, Guid.NewGuid(), Guid.NewGuid(), null, "123", 10m, "Paid", null, DateTime.UtcNow, DateTime.UtcNow);
            _mockRepository.Setup(r => r.GetOrderAsync(orderId)).ReturnsAsync(order);

            // Act
            var result = await _orderService.GetOrderAsync(orderId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(order.Id, result.Id);
            Assert.AreEqual(order.UserId, result.UserId);
            Assert.AreEqual(order.RestaurantId, result.RestaurantId);
            Assert.AreEqual(order.Status, result.Status);
            Assert.AreEqual(order.OrderNumber, result.OrderNumber);
        }

        [TestMethod]
        public async Task GetOrdersForUserAsync_ShouldReturnOrdersForUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orders = new List<Order>
            {
            new Order(Guid.NewGuid(),userId,Guid.NewGuid(),null,"123",10m,"Created",null,DateTime.UtcNow,DateTime.UtcNow),
            new Order(Guid.NewGuid(),userId,Guid.NewGuid(),null,"124",10m,"Paid",null,DateTime.UtcNow,DateTime.UtcNow),
            new Order(Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),null,"125",10m,"Created",null,DateTime.UtcNow,DateTime.UtcNow)
            };
            _mockRepository.Setup(r => r.GetOrdersForUserAsync(userId)).ReturnsAsync(orders.Where(o => o.UserId.Equals(userId)).ToList());

            // Act
            var result = await _orderService.GetOrdersForUserAsync(userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(o => o.UserId == userId));
        }

        [TestMethod]
        public async Task GetOrdersForRestaurantAsync_ShouldReturnOrdersForRestaurant()
        {
            // Arrange
            var restaurantId = Guid.NewGuid();
            var orders = new List<Order>
            {
            new Order(Guid.NewGuid(),Guid.NewGuid(),restaurantId,null,"123",10m,"Created",null,DateTime.UtcNow,DateTime.UtcNow),
            new Order(Guid.NewGuid(),Guid.NewGuid(),restaurantId,null,"124",10m,"Paid",null,DateTime.UtcNow,DateTime.UtcNow),
            new Order(Guid.NewGuid(),Guid.NewGuid(),Guid.NewGuid(),null,"125",10m,"Created",null,DateTime.UtcNow,DateTime.UtcNow)
            };
            _mockRepository.Setup(r => r.GetOrdersForRestaurantAsync(restaurantId)).ReturnsAsync(orders.Where(o => o.RestaurantId.Equals(restaurantId)).ToList());

            // Act
            var result = await _orderService.GetOrdersForRestaurantAsync(restaurantId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(o => o.RestaurantId == restaurantId));
        }

        [TestMethod]
        public async Task UpdateOrderAsync_ShouldUpdateOrderStatus()
        {
            // Arrange
            var order = new Order(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, "123", 10m, "Created", null, DateTime.UtcNow, DateTime.UtcNow);
            var updatedStatus = "Paid";

            _mockRepository.Setup(r => r.GetOrderAsync(order.Id)).ReturnsAsync(order);
            _mockRepository.Setup(r => r.UpdateOrderAsync(order)).Returns(Task.CompletedTask);

            // Act
            await _orderService.UpdateOrderStatusAsync(order.Id, updatedStatus);

            // Assert
            _mockRepository.Verify(r => r.GetOrderAsync(order.Id), Times.Once);
            _mockRepository.Verify(r => r.UpdateOrderAsync(order), Times.Once);
            Assert.AreEqual(updatedStatus, order.Status);
        }


        [TestMethod]
        public async Task DeleteOrderAsync_ShouldDeleteOrder()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order(orderId, Guid.NewGuid(), Guid.NewGuid(), null, "123", 10m, "Created", null, DateTime.UtcNow, DateTime.UtcNow);
            _mockRepository.Setup(r => r.GetOrderAsync(orderId)).ReturnsAsync(order);

            // Act
            await _orderService.DeleteOrderAsync(orderId);

            // Assert
            _mockRepository.Verify(r => r.DeleteOrderAsync(order), Times.Once);
        }

    }

}
