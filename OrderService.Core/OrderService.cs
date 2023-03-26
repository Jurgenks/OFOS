using OFOS.Domain.Models;

namespace OrderService.Core
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task CreateOrderAsync(Guid userId, Guid restaurantId, List<Product> products)
        {
            // Calculate total price based on the products
            var totalPrice = 0m;
            foreach (var product in products)
            {
                totalPrice += product.Price;
            }

            // Create order
            var order = new Order(userId, restaurantId, "OrderNumber", totalPrice, "Created", products);

            // Save order to repository
            await _orderRepository.CreateOrderAsync(order);
        }

        public async Task<Order?> GetOrderAsync(Guid orderId)
        {
            return await _orderRepository.GetOrderAsync(orderId);
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

    }



}
