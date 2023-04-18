using OFOS.Domain.Models;

namespace NotificationService.Core
{
    public interface INotificationService
    {
        void ConsumeEmailMessage(string emailMessage);
        void SendEmailMessage(EmailMessage mail);
        Task StartAsync();
    }
}
