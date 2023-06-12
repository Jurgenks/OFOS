using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OFOS.Domain.Models;
using System.Security.Claims;
using UserService.Controllers;
using UserService.Core;
using static UserService.Controllers.UserController;

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

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, expectedUser.Id.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext
            {
                User = principal
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

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

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext
            {
                User = principal
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

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

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, updatedUser.Id.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext
            {
                User = principal
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.UpdateUser(updatedUser);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
            _mockUserService.Verify(x => x.UpdateUser(updatedUser.Id, updatedUser), Times.Once);
        }

        [TestMethod]
        public async Task UpdateUser_ReturnsBadRequest_WhenIdDoesNotMatch()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updatedUser = new User("John", "Doe", "johndoe@example.com", null, "test", "test", "test", "test", "test", "password123");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext
            {
                User = principal
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.UpdateUser(updatedUser);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ForbidResult));
        }

        [TestMethod]
        public async Task DeleteUser_ReturnsNoContent_WhenDeleteSuccessful()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext
            {
                User = principal
            };         

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

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
            var result = await _controller.Authenticate(user.Email,user.Password);

            // Assert
            var okResult = result as UnauthorizedResult;
            Assert.IsNotNull(okResult);
        }

        [TestMethod]
        public async Task Authenticate_ReturnsOk_WhenUserFound()
        {
            // Arrange
            string jwtToken = "JwtTokenTest";
            var expectedUser = new User("John", "Doe", "johndoe@example.com", null, "test", "test", "test", "test", "test", "password123");
            _mockUserService.Setup(x => x.Authenticate(expectedUser.Email, expectedUser.Password)).ReturnsAsync(jwtToken);

            // Create a new HttpContext object with a mock response object
            var httpContext = new DefaultHttpContext();
            var response = new Mock<HttpResponse>();
            httpContext.Response.StatusCode = 200;
            response.SetupProperty(x => x.StatusCode);
            httpContext.Response.Body = new MemoryStream();
            response.SetupProperty(x => x.Body);
            httpContext.Response.Cookies.Append("jwt", jwtToken);
            response.Setup(x => x.Cookies).Returns(httpContext.Response.Cookies);

            // Set the HttpContext of the controller to the mock HttpContext
            _controller.ControllerContext.HttpContext = httpContext;

            // Act
            var result = await _controller.Authenticate(expectedUser.Email,expectedUser.Password);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Value);
            Assert.AreEqual(okResult.Value, jwtToken);
        }


        [TestMethod]
        public async Task ForgotPassword_ReturnsBadRequest_WhenEmailIsNullOrEmpty()
        {
            // Arrange
            string email = null;

            // Act
            var result = await _controller.ForgotPassword(email);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task ForgotPassword_ReturnsOk_WhenUserDoesNotExist()
        {
            // Arrange
            string email = "test@example.com";

            _mockUserService.Setup(x => x.GetUserByEmail(email)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.ForgotPassword(email);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public async Task ForgotPassword_ReturnsOk_WhenUserExists()
        {
            // Arrange
            string email = "test@example.com";
            var user = new User("John", "Doe", email, null, "test", "test", "test", "test", "test", "password123");
            _mockUserService.Setup(x => x.GetUserByEmail(email)).ReturnsAsync(user);

            // Act
            var result = await _controller.ForgotPassword(email);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public async Task ForgotPassword_WhenCalledWithValidEmail_ShouldSendPasswordResetMessageToRabbitMQ()
        {
            // Arrange
            string email = "test@example.com";
            var user = new User("John", "Doe", email, null, "test", "test", "test", "test", "test", "password123");
            _mockUserService.Setup(u => u.GetUserByEmail(email)).ReturnsAsync(user);

            // Act
            var result = await _controller.ForgotPassword(email);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public async Task ResetPassword_ReturnsBadRequest_WhenEmailIsNullOrEmpty()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext
            {
                User = principal
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };


            // Act
            var result = await _controller.ResetPassword(null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task ResetPassword_ReturnsBadRequest_WhenTokenIsNullOrEmpty()
        {
            // Arrange
            string newPassword = "new-password";


            // Act
            var result = await _controller.ResetPassword(newPassword);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

    }
}
