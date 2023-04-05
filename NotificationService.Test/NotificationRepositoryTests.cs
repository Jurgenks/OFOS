using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NotificationService.Core;
using NotificationService.Data;
using OFOS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationService.Test
{
    [TestClass]
    public class NotificationRepositoryTests
    {
        private NotificationDbContext _dbContext;
        private NotificationRepository _notificationRepository;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<NotificationDbContext>()
                .UseInMemoryDatabase(databaseName: "NotificationDb")
                .Options;

            _dbContext = new NotificationDbContext(options);
            _notificationRepository = new NotificationRepository(_dbContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [TestMethod]
        public async Task GetByIdAsync_ShouldReturnCorrectNotification()
        {
            // Arrange
            var notification = new Notification(Guid.NewGuid(), "test", "test", "test");

            // Act
            await _notificationRepository.CreateAsync(notification);

            var result = await _notificationRepository.GetByIdAsync(notification.Id);

            // Assert
            Assert.AreEqual(notification, result);
        }

        [TestMethod]
        public async Task GetByUserIdAsync_ShouldReturnCorrectNotifications()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var notifications = new List<Notification>
            {
                new Notification(userId, "test", "test", "test"),
                new Notification(userId, "test", "test", "test"),
                new Notification(Guid.NewGuid(), "test", "test", "test"),
            };

            await _notificationRepository.CreateAsync(notifications[0]);
            await _notificationRepository.CreateAsync(notifications[1]);
            await _notificationRepository.CreateAsync(notifications[2]);

            // Act
            var result = await _notificationRepository.GetByUserIdAsync(userId);
            var expected = notifications.Where(n => n.UserId == userId).ToList();

            // Assert
            CollectionAssert.AreEquivalent(expected, result.ToList());
        }

        [TestMethod]
        public async Task CreateAsync_ShouldCreateNewNotification()
        {
            // Arrange
            var notification = new Notification(Guid.NewGuid(), "test", "test", "test");


            // Act
            await _notificationRepository.CreateAsync(notification);

            // Assert
            Assert.IsTrue(_dbContext.Notifications.Contains(notification));
        }

        [TestMethod]
        public async Task DeleteAsync_ShouldDeleteExistingNotification()
        {
            // Arrange
            var notification = new Notification(Guid.NewGuid(), "test", "test", "test");

            // Act
            await _notificationRepository.CreateAsync(notification);
            await _notificationRepository.DeleteAsync(notification.Id);

            // Assert
            Assert.IsFalse(_dbContext.Notifications.Contains(notification));
        }
    }
}
