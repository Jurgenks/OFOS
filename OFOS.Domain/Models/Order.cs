namespace OFOS.Domain.Models
{
    public class Order
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; set; }
        public Guid RestaurantId { get; set; }
        public Delivery? Delivery { get; set; }
        public string? OrderNumber { get; set; }
        public decimal? TotalPrice { get; set; }
        public string? Status { get; set; }
        public List<Product> Products { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        //Call this when creating an Order
        public Order(Guid userId, Guid restaurantId, string orderNumber, decimal totalPrice, string status, List<Product> products)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            RestaurantId = restaurantId;
            OrderNumber = orderNumber;
            TotalPrice = totalPrice;
            Status = status;
            Products = products;
            CreatedAt = DateTime.UtcNow;
        }

        //Call this when calling an Order
        public Order(Guid id, Guid userId, Guid restaurantId, Delivery? delivery, string orderNumber, decimal totalPrice, string status, List<Product> products, DateTime createdAt, DateTime updatedAt)
        {
            Id = id;
            UserId = userId;
            RestaurantId = restaurantId;
            Delivery = delivery;
            OrderNumber = orderNumber;
            TotalPrice = totalPrice;
            Status = status;
            Products = products;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        public Order()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            Products = new();
        }
    }
}
