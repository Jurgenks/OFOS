using Microsoft.AspNetCore.Mvc;
using Moq;
using OFOS.Domain.Models;
using UserService.Controllers;
using UserService.Core;

namespace UserService.Test
{
    [TestClass]
    public class UserControllerTests
    {
        private Mock<IUserService> _mockUserService;
        private UserController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UserController(_mockUserService.Object);
        }

        [TestMethod]
        public async Task GetUser_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var expectedUser = new User("John", "Doe", "johndoe@example.com", null, "test", "test", "test", "test", "test", "password123");
            _mockUserService.Setup(x => x.GetUser(expectedUser.Id)).ReturnsAsync(expectedUser);

            // Act
            var result = await _controller.GetUser(expectedUser.Id);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Value);
            var user = okResult.Value as User;
            Assert.AreEqual(expectedUser.Id, user.Id);
        }

        [TestMethod]
        public async Task GetUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            var okResult = result.Result as NotFoundResult;
            Assert.IsNotNull(okResult);
        }

        [TestMethod]
        public async Task UpdateUser_ReturnsNoContent_WhenUpdateSuccessful()
        {
            // Arrange
            var updatedUser = new User("John", "Doe", "johndoe@example.com", null, "test", "test", "test", "test", "test", "password123");
            // Act
            var result = await _controller.UpdateUser(updatedUser.Id, updatedUser);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockUserService.Verify(x => x.UpdateUser(updatedUser), Times.Once);
        }

        [TestMethod]
        public async Task UpdateUser_ReturnsBadRequest_WhenIdDoesNotMatch()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updatedUser = new User("John", "Doe", "johndoe@example.com", null, "test", "test", "test", "test", "test", "password123");

            // Act
            var result = await _controller.UpdateUser(userId, updatedUser);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task DeleteUser_ReturnsNoContent_WhenDeleteSuccessful()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockUserService.Verify(x => x.DeleteUser(userId), Times.Once);
        }

        [TestMethod]
        public async Task CreateUser_ReturnsOk_WhenUserCreated()
        {
            // Arrange
            var newUser = new User("John", "Doe", "johndoe@example.com", null, "test", "test", "test", "test", "test", "password123");
            _mockUserService.Setup(x => x.CreateUser(newUser)).Verifiable();

            // Act
            var result = await _controller.CreateUser(newUser);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual("User: " + newUser.Id + " is created", okResult.Value);
            _mockUserService.Verify(x => x.CreateUser(newUser), Times.Once);
        }

        [TestMethod]
        public async Task CreateUser_ReturnsBadRequest_WhenModelIsNull()
        {
            // Arrange
            User model = null;

            // Act
            var result = await _controller.CreateUser(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }


        [TestMethod]
        public async Task Authenticate_ReturnsUnauthorized_WhenUserNotFound()
        {
            // Arrange
            var user = new User("John", "Doe", "johndoe@example.com", null, "test", "test", "test", "test", "test", "password123");

            // Act
            var result = await _controller.Authenticate(user);

            // Assert
            var okResult = result.Result as UnauthorizedResult;
            Assert.IsNotNull(okResult);
        }

        [TestMethod]
        public async Task Authenticate_ReturnsOk_WhenUserFound()
        {
            // Arrange
            string jwtToken = "JwtTokenTest";
            var expectedUser = new User("John", "Doe", "johndoe@example.com", null, "test", "test", "test", "test", "test", "password123");
            _mockUserService.Setup(x => x.Authenticate(expectedUser.Email, expectedUser.Password)).ReturnsAsync(jwtToken);

            // Act
            var result = await _controller.Authenticate(expectedUser);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Value);
            Assert.AreEqual(okResult.Value, jwtToken);
        }

    }
}
