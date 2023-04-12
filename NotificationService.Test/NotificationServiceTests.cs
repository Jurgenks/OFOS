using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NotificationService.Core;
using OFOS.Domain.Models;
using RabbitMQ.Client;
using System;
using Service = NotificationService.Core.NotificationService;

namespace NotificationService.Test
{
    [TestClass]
    public class NotificationServiceTests
    {
        private Mock<INotificationRepository> _notificationRepositoryMock;
        private Mock<IConnection> _mockConnection;
        private Mock<IModel> _mockChannel;
        private Service _notificationService;

        [TestInitialize]
        public void Setup()
        {
            _notificationRepositoryMock = new Mock<INotificationRepository>();
            _mockConnection = new Mock<IConnection>();
            _mockChannel = new Mock<IModel>();

            _mockConnection.Setup(c => c.CreateModel())
                           .Returns(_mockChannel.Object);

            _notificationService = new Service(_notificationRepositoryMock.Object,_mockConnection.Object);
        }

        [TestMethod]
        public void ConsumeEmailMessage_ValidEmailMessage_SuccessfullyCreatesNotification()
        {
            // Arrange
            var emailMessage = "{\"to\":\"test@test.com\",\"subject\":\"Test email\",\"body\":\"This is a test email\"}";
            _notificationRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Notification>())).Verifiable();

            // Act
            _notificationService.ConsumeEmailMessage(emailMessage);

            // Assert
            _notificationRepositoryMock.Verify(x => x.CreateAsync(It.Is<Notification>(n =>
                n.Message == emailMessage &&
                n.Type == "Email" &&
                n.Status == "Received"
            )), Times.Once);
        }
    }
}
