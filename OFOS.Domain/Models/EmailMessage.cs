namespace OFOS.Domain.Models
{
    public class EmailMessage
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public EmailMessage() { }

        public EmailMessage(string to, string subject, string body)
        {
            To = to ?? throw new ArgumentNullException(nameof(to));
            Subject = subject ?? throw new ArgumentNullException(nameof(subject));
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }
    }

}
