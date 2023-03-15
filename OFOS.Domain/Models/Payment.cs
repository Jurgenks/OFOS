namespace OFOS.Domain.Models
{
    public class Payment
    {
        public Guid Id { get; private set; }
        public Guid OrderId { get; set; }
        public string ReferenceNumber { get; set; }
        public string Method { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; set; }

        public Payment(Guid orderId, string referenceNumber, string method, string status, decimal amount)
        {
            Id = Guid.NewGuid();
            ReferenceNumber = referenceNumber;
            OrderId = orderId;
            Method = method;
            Status = status;
            Amount = amount;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
