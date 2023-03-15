namespace OFOS.Domain.Models
{
    public class Notification
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; set; }

        public Notification(Guid userId, string message, string type, string status)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Message = message;
            Type = type;
            Status = status;
            CreatedAt = DateTime.UtcNow;
        }
    }

}
