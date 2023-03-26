using Microsoft.EntityFrameworkCore;
using OFOS.Domain.Models;
using UserService.Core;
using UserService.Data;

namespace UserService.Test
{
    [TestClass]
    public class UserRepositoryTests
    {
        private UserDbContext _dbContext;
        private UserRepository _userRepository;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(databaseName: "UserService_Test_Database")
                .Options;
            _dbContext = new UserDbContext(options);
            _userRepository = new UserRepository(_dbContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [TestMethod]
        public async Task GetByIdAsync_ExistingUserId_ReturnsUser()
        {
            // Arrange
            var email = "johndoe@example.com";
            var user = new User("John", "Doe", email, null, "test", "test", "test", "test", "test", "password123");
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userRepository.GetByIdAsync(user.Id);

            // Assert
            Assert.AreEqual(user, result);
        }

        [TestMethod]
        public async Task GetByIdAsync_NonExistingUserId_ReturnsNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act
            var result = await _userRepository.GetByIdAsync(nonExistingId);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetByEmailAsync_ExistingEmail_ReturnsUser()
        {
            // Arrange
            var email = "johndoe@example.com";
            var user = new User("John", "Doe", email, null, "test", "test", "test", "test", "test", "password123");
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userRepository.GetByEmailAsync(email);

            // Assert
            Assert.AreEqual(user, result);
        }

        [TestMethod]
        public async Task GetByEmailAsync_NonExistingEmail_ReturnsNull()
        {
            // Arrange
            var nonExistingEmail = "nonexistingemail@example.com";

            // Act
            var result = await _userRepository.GetByEmailAsync(nonExistingEmail);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task CreateAsync_ValidUser_AddsUserToDatabase()
        {
            // Arrange
            var email = "johndoe@example.com";
            var user = new User("John", "Doe", email, null, "test", "test", "test", "test", "test", "password123");

            // Act
            await _userRepository.CreateAsync(user);
            var result = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user, result);
        }

        [TestMethod]
        public async Task UpdateAsync_ExistingUser_UpdatesUserInDatabase()
        {
            // Arrange
            var email = "johndoe@example.com";
            var user = new User("John", "Doe", email, null, "test", "test", "test", "test", "test", "password123");
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Update user
            user.FirstName = "Jane";
            user.LastName = "Doe";

            // Act
            await _userRepository.UpdateAsync(user);
            var result = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.FirstName, result.FirstName);
            Assert.AreEqual(user.LastName, result.LastName);

        }

        [TestMethod]
        public async Task DeleteAsync_ExistingUserId_DeletesUser()
        {
            // Arrange
            var email = "johndoe@example.com";
            var user = new User("John", "Doe", email, null, "test", "test", "test", "test", "test", "password123");

            // Act
            await _userRepository.DeleteAsync(user.Id);

            // Assert
            var deletedUser = await _dbContext.Users.FindAsync(user.Id);
            Assert.IsNull(deletedUser);
        }

        [TestMethod]
        public async Task DeleteAsync_NonExistingUserId_DoesNothing()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            await _userRepository.DeleteAsync(userId);

            // Assert
            var deletedUser = await _dbContext.Users.FindAsync(userId);
            Assert.IsNull(deletedUser);
        }
    }
}