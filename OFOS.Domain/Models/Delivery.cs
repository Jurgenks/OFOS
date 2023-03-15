namespace OFOS.Domain.Models
{
    public class Delivery
    {
        public Guid Id { get; private set; }
        public Guid OrderId { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; set; }

        public Delivery(Guid orderId, string address, string city, string region, string postalCode, string country, string contactName, string contactPhone, string status)
        {
            Id = Guid.NewGuid();
            OrderId = orderId;
            Address = address;
            City = city;
            Region = region;
            PostalCode = postalCode;
            Country = country;
            ContactName = contactName;
            ContactPhone = contactPhone;
            Status = status;
            CreatedAt = DateTime.UtcNow;
        }
    }

}
