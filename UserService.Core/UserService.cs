using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using OFOS.Domain.Models;
using RabbitMQ.Client;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace UserService.Core
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IConnection _rabbitConnection;
        private readonly IModel _rabbitChannel;

        public UserService(IUserRepository userRepository, IConfiguration configuration, IConnection rabbitConnection)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _rabbitConnection = rabbitConnection;
            _rabbitChannel = _rabbitConnection.CreateModel();
        }

        public async Task CreateUser(User user)
        {
            // Ensure that the user's ID is a valid GUID
            if (user.Id != Guid.Empty && user.Password != null)
            {
                // Encrypt the user's password before saving to the database
                user.Password = EncryptPassword(user.Password);

                await _userRepository.CreateAsync(user);
            }
        }

        public async Task<User?> GetUser(Guid id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task UpdateUser(User user)
        {
            // Retrieve the original user from the database
            var originalUser = await _userRepository.GetByIdAsync(user.Id);

            // Update the user's properties
            originalUser.FirstName = user.FirstName;
            originalUser.LastName = user.LastName;
            originalUser.Email = user.Email;
            originalUser.Address = user.Address;
            originalUser.RetrievalToken = user.RetrievalToken;
            originalUser.AuthenticationToken = user.AuthenticationToken;

            // Encrypt the user's password before saving to the database
            originalUser.Password = EncryptPassword(user.Password);

            await _userRepository.UpdateAsync(originalUser);
        }

        public async Task DeleteUser(Guid id)
        {
            await _userRepository.DeleteAsync(id);
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<string?> Authenticate(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            // Check if the user exists and the password is correct
            if (user != null && VerifyPassword(password, user.Password))
            {
                var token = GenerateJwtToken(user);

                user.AuthenticationToken = token;

                await UpdateUser(user);

                return token;
            }

            return null;
        }

        public async Task ResetPassword(User user, string token, string newPassword)
        {
            var originalUser = GetUser(user.Id).Result;

            if (originalUser != null)
            {
                originalUser.Password = newPassword;

                await UpdateUser(originalUser);
            }
        }

        public async Task SendResetToken(User user)
        {
            // Generate password reset token
            var token = GenerateJwtToken(user);

            //Update the user with the new retrievalToken
            user.RetrievalToken = token;
            await UpdateUser(user);

            // Create email message
            var emailMessage = new EmailMessage
            {
                To = user.Email,
                Subject = "Password reset",
                Body = $"Click this link to reset your password: {"[RESETLINK]"}"
            };

            // Serialize email message as message body
            var messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(emailMessage));

            // Publish message to RabbitMQ 
            _rabbitChannel.BasicPublish(exchange: "",
                                  routingKey: "email-queue",
                                  basicProperties: null,
                                  body: messageBody);
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Name, $"{user.FirstName[..1]}{"."}{user.LastName}"),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static string EncryptPassword(string password)
        {
            // Implement password encryption logic here
            password = AesEncryptor.Encrypt(password);
            return password;
        }

        private static bool VerifyPassword(string password, string hashedPassword)
        {
            // Implement password verification logic here
            hashedPassword = AesEncryptor.Decrypt(hashedPassword);
            return password == hashedPassword;
        }


    }


}
