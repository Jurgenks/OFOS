using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OFOS.Domain.Models;
using RabbitMQ.Client;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace OrderService.Core
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IConfiguration _configuration;
        private readonly IConnection _rabbitConnection;
        private readonly IModel _rabbitChannel;

        public OrderService(IOrderRepository orderRepository, IConfiguration configuration, IConnection rabbitConnection)
        {
            _orderRepository = orderRepository;
            _configuration = configuration;
            _rabbitConnection = rabbitConnection;
            _rabbitChannel = _rabbitConnection.CreateModel();
        }

        public async Task<string> CreateOrderAsync(ClaimsPrincipal user, Guid restaurantId, List<Product> products)
        {
            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userEmail = user.FindFirst(ClaimTypes.Email)?.Value;
            var userName = user.FindFirst(ClaimTypes.Name)?.Value;

            // Calculate total price based on the products
            var totalPrice = 0m;
            foreach (var product in products)
            {
                totalPrice += product.Price;
            }

            // Create order
            string orderNumber = await GenerateOrderNumberAsync();

            var order = new Order(userId, restaurantId, orderNumber, totalPrice, "Created", products);

            // Save order to repository
            await _orderRepository.CreateOrderAsync(order);

            // Create email message
            var emailMessage = new EmailMessage
            {
                To = userEmail,
                Subject = "Order: " + orderNumber + " is placed",
                Body = "Dear " + userName + ",\r\n\r\nYour order has been placed\r\n\r\nBest regards,\r\nTeam OFOS"
            };

            // Serialize email message as message body
            var messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(emailMessage));

            // Publish message to RabbitMQ 
            _rabbitChannel.BasicPublish(exchange: "",
                                  routingKey: "email-queue",
                                  basicProperties: null,
                                  body: messageBody);

            // Return OrderNumber
            return orderNumber;
        }

        public async Task<Order?> GetOrderAsync(Guid orderId)
        {
            return await _orderRepository.GetOrderAsync(orderId);
        }

        public async Task<Order?> GetOrderByOrderNumberAsync(string orderNumber)
        {
            return await _orderRepository.GetOrderByOrderNumberAsync(orderNumber);
        }

        public async Task<List<Order>?> GetOrdersForUserAsync(Guid userId)
        {
            return await _orderRepository.GetOrdersForUserAsync(userId);
        }

        public async Task<List<Order>?> GetOrdersForRestaurantAsync(Guid restaurantId)
        {
            return await _orderRepository.GetOrdersForRestaurantAsync(restaurantId);
        }

        public async Task<Order> UpdateOrderStatusAsync(Guid orderId, string newStatus)
        {
            var order = await _orderRepository.GetOrderAsync(orderId);

            if (order == null)
            {
                throw new Exception($"Order with id {orderId} not found");
            }

            if (newStatus == "Paid" && order.Status != "Created")
            {
                throw new Exception($"Order with id {orderId} cannot be paid because it is already in status {order.Status}");
            }

            order.Status = newStatus;

            await _orderRepository.UpdateOrderAsync(order);

            return order;
        }

        public async Task DeleteOrderAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderAsync(orderId);

            if (order == null)
            {
                throw new ArgumentException($"Order with ID {orderId} not found.");
            }

            await _orderRepository.DeleteOrderAsync(order);
        }

        private async Task<string> GenerateOrderNumberAsync()
        {
            int orderCount = await _orderRepository.GetOrdersCountAsync();
            string year = DateTime.UtcNow.Year.ToString();
            string month = DateTime.UtcNow.Month.ToString().PadLeft(2, '0');
            string day = DateTime.UtcNow.Day.ToString().PadLeft(2, '0');
            string hour = DateTime.UtcNow.Hour.ToString().PadLeft(2, '0');
            string minute = DateTime.UtcNow.Minute.ToString().PadLeft(2, '0');
            string second = DateTime.UtcNow.Second.ToString().PadLeft(2, '0');
            string orderNumber = $"{year}{month}{day}-{hour}{minute}{second}-{orderCount + 1}";
            return orderNumber;
        }


    }



}
