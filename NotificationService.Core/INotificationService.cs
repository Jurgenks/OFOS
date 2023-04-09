namespace NotificationService.Core
{
    public interface INotificationService
    {
        void ConsumeEmailMessage(string emailMessage);
    }
}
