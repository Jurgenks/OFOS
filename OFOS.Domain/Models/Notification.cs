namespace OFOS.Domain.Models
{
    public class Notification
    {
        public Guid Id { get; private set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; set; }

        public Notification(string message, string type, string status)
        {
            Id = Guid.NewGuid();
            Message = message;
            Type = type;
            Status = status;
            CreatedAt = DateTime.UtcNow;
        }
    }

}
