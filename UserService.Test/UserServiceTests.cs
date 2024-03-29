using Microsoft.Extensions.Configuration;
using Moq;
using OFOS.Domain.Models;
using RabbitMQ.Client;
using UserService.Core;
using Service = UserService.Core.UserService;

namespace UserService.Test
{
    [TestClass]
    public class UserServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<IConnection> _mockConnection;
        private Mock<IModel> _mockChannel;
        private Service _userService;

        [TestInitialize]
        public void Setup()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _configurationMock = new Mock<IConfiguration>();
            _mockConnection = new Mock<IConnection>();
            _mockChannel = new Mock<IModel>();
            _mockConnection.Setup(c => c.CreateModel())
                           .Returns(_mockChannel.Object);

            _userService = new Service(_userRepositoryMock.Object, _configurationMock.Object, _mockConnection.Object);
        }

        [TestMethod]
        public async Task CreateUser_ValidUser_CallsUserRepositoryCreateAsync()
        {
            // Arrange
            var email = "johndoe@example.com";
            var user = new User("John", "Doe", email, null, null, null, null, null, null, "password123");

            _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            await _userService.CreateUser(user);

            // Assert
            _userRepositoryMock.Verify(x => x.CreateAsync(user), Times.Once);
        }

        [TestMethod]
        public async Task GetUser_ExistingUserId_ReturnsUser()
        {
            // Arrange
            var email = "johndoe@example.com";
            var user = new User("John", "Doe", email, null, null, null, null, null, null, "password123");

            _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.GetUser(user.Id);

            // Assert
            Assert.AreEqual(user, result);
        }

        [TestMethod]
        public async Task UpdateUser_ExistingUser_CallsUserRepositoryUpdateAsync()
        {
            // Arrange
            var email = "johndoe@example.com";
            var user = new User("John", "Doe", email, null, null, null, null, null, null, "password123");

            _userRepositoryMock.Setup(x => x.GetByIdAsync(user.Id))
                .ReturnsAsync(user);
            _userRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Act
            await _userService.UpdateUser(user.Id,user);

            // Assert
            _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }

        [TestMethod]
        public async Task DeleteUser_ExistingUserId_CallsUserRepositoryDeleteAsync()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepositoryMock.Setup(x => x.DeleteAsync(userId))
                .Returns(Task.CompletedTask);

            // Act
            await _userService.DeleteUser(userId);

            // Assert
            _userRepositoryMock.Verify(x => x.DeleteAsync(userId), Times.Once);
        }

        [TestMethod]
        public async Task GetUserByEmail_ExistingEmail_ReturnsUser()
        {
            // Arrange
            var email = "johndoe@example.com";
            var user = new User("John", "Doe", email, null, null, null, null, null, null, "password123");

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserByEmail(email);

            // Assert
            Assert.AreEqual(user, result);
        }
    }

}