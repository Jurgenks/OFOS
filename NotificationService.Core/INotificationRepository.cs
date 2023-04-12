using OFOS.Domain.Models;

namespace NotificationService.Core
{
    public interface INotificationRepository
    {
        Task<Notification?> GetByIdAsync(Guid id);
        Task CreateAsync(Notification notification);
        Task DeleteAsync(Guid id);
    }
}
