using OFOS.Domain.Models;

namespace UserService.Core
{
    public interface IUserService
    {
        Task CreateUser(User user);
        Task<User?> GetUser(Guid id);
        Task UpdateUser(User user);
        Task DeleteUser(Guid id);
        Task<User?> GetUserByEmail(string email);
        Task<string?> Authenticate(string email, string password);
        Task ResetPassword(Guid userId, string newPassword);
        void SendResetToken(User user);
    }
}
