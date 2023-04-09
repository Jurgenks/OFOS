using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using OFOS.Domain.Models;

namespace NotificationService.Core
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _dbContext;

        public NotificationRepository(NotificationDbContext context)
        {
            _dbContext = context;
        }

        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Notifications.SingleOrDefaultAsync(n => n.Id == id);
        }

        public async Task CreateAsync(Notification notification)
        {
            await _dbContext.Notifications.AddAsync(notification);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var notification = await GetByIdAsync(id);
            if (notification != null)
            {
                _dbContext.Notifications.Remove(notification);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
