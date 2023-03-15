using Microsoft.AspNetCore.Mvc;
using Moq;
using OFOS.Domain.Models;
using OrderService.Controllers;
using OrderService.Core;

namespace OrderService.Test
{
    [TestClass]
    public class OrderServiceControllerTests
    {
        private Mock<IOrderService> _mockOrderService;
        private OrderServiceController _controller;

        [TestInitialize]
        public void Initialize()
        {
            _mockOrderService = new Mock<IOrderService>();
            _controller = new OrderServiceController(_mockOrderService.Object);
        }

        [TestMethod]
        public async Task CreateOrderAsync_ReturnsCreatedAtAction()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var restaurantId = Guid.NewGuid();
            var products = new List<Product>()
            {
                new Product("Product 1","Text", 10m,100),
                new Product("Product 2","Text", 20m,200)
            };

            // Act
            var result = await _controller.CreateOrderAsync(userId, restaurantId, products);

            // Assert
            Assert.IsNotNull(result);
        }


        [TestMethod]
        public async Task GetOrderAsync_ReturnsOkObjectResult()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order(orderId, Guid.NewGuid(), Guid.NewGuid(), null, "OrderNumber", 10m, "Created", new List<Product>(), DateTime.UtcNow, DateTime.UtcNow);
            _mockOrderService.Setup(x => x.GetOrderAsync(orderId)).ReturnsAsync(order);

            // Act
            var result = await _controller.GetOrderAsync(orderId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okObjectResult = (OkObjectResult)result;
            Assert.IsInstanceOfType(okObjectResult.Value, typeof(Order));
            var returnedOrder = (Order)okObjectResult.Value;
            Assert.AreEqual(order.Id, returnedOrder.Id);
            Assert.AreEqual(order.UserId, returnedOrder.UserId);
            Assert.AreEqual(order.RestaurantId, returnedOrder.RestaurantId);
            Assert.AreEqual(order.TotalPrice, returnedOrder.TotalPrice);
            Assert.AreEqual(order.Status, returnedOrder.Status);
        }

        [TestMethod]
        public async Task GetOrdersForUserAsync_ReturnsOkObjectResult()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orders = new List<Order>
            {
            new Order(Guid.NewGuid(), userId, Guid.NewGuid(), null, "OrderNumber1", 10m, "Created", new List<Product>(),DateTime.UtcNow,DateTime.UtcNow),
            new Order(Guid.NewGuid(), userId, Guid.NewGuid(), null, "OrderNumber2", 20m, "Paid", new List<Product>(),DateTime.UtcNow,DateTime.UtcNow)
            };
            _mockOrderService.Setup(x => x.GetOrdersForUserAsync(userId)).ReturnsAsync(orders);

            // Act
            var result = await _controller.GetOrdersForUserAsync(userId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okObjectResult = (OkObjectResult)result;
            Assert.IsInstanceOfType(okObjectResult.Value, typeof(List<Order>));
            var returnedOrders = (List<Order>)okObjectResult.Value;
            Assert.AreEqual(2, returnedOrders.Count);
            Assert.IsTrue(returnedOrders.All(o => o.UserId == userId));
        }

        [TestMethod]
        public async Task UpdateOrderStatusAsync_ReturnsOkObjectResult()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var newStatus = "Paid";
            _mockOrderService.Setup(s => s.UpdateOrderStatusAsync(orderId, newStatus)).ReturnsAsync(new Order(orderId, Guid.NewGuid(), Guid.NewGuid(), null, "test", 10m, newStatus, new(), DateTime.UtcNow, DateTime.UtcNow));

            // Act
            var result = await _controller.UpdateOrderStatusAsync(orderId, newStatus) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            var order = result.Value as Order;
            Assert.IsNotNull(order);
            Assert.AreEqual(orderId, order.Id);
            Assert.AreEqual(newStatus, order.Status);
        }

    }

}
