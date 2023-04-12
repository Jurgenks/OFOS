using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NotificationService.Core;
using NotificationService.Controllers;
using System.Threading.Tasks;
using OFOS.Domain.Models;

namespace NotificationService.Tests
{
    [TestClass]
    public class NotificationControllerTests
    {
        private Mock<INotificationService> _notificationServiceMock;
        private NotificationController _notificationController;

        [TestInitialize]
        public void Setup()
        {
            _notificationServiceMock = new Mock<INotificationService>();
            _notificationController = new NotificationController(_notificationServiceMock.Object);
        }

        [TestMethod]
        public async Task SendEmail_ValidModel_ReturnsOkResult()
        {
            // Arrange
            var emailModel = new EmailMessage("test@example.com", "Test Subject", "Test Body");

            // Act
            var result = _notificationController.SendEmail(emailModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public async Task SendEmail_InvalidModel_ReturnsBadRequestResult()
        {
            // Arrange
            var emailModel = new EmailMessage("test", "test subject", "test body");
            emailModel.To = null;
            _notificationController.ModelState.AddModelError("To", "The To field is required.");

            // Act
            var result = _notificationController.SendEmail(emailModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestCleanup]
        public void Cleanup()
        {
            _notificationServiceMock.VerifyAll();
        }
    }
}
