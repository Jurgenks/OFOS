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

                // Create email message
                var emailMessage = new EmailMessage
                {
                    To = user.Email,
                    Subject = "Thank You for Joining OFOS!",
                    Body = "Dear " + user.FirstName + ",\r\n\r\nI wanted to take a moment to thank you for creating an account on the OFOS platform. We're excited to have you join our community of food enthusiasts!\r\n\r\nWith your OFOS account, you'll be able to explore new recipes, connect with other foodies, and share your own culinary creations. Whether you're a seasoned chef or a newbie in the kitchen, we're confident you'll find something to love on our platform.\r\n\r\nIf you have any questions or feedback as you get started on OFOS, please don't hesitate to reach out to our support team. We're here to help!\r\n\r\nThanks again for joining OFOS. We can't wait to see what delicious dishes you'll whip up next!\r\n\r\nBest regards,\r\nTeam OFOS"
                };

                // Serialize email message as message body
                var messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(emailMessage));

                // Publish message to RabbitMQ 
                _rabbitChannel.BasicPublish(exchange: "",
                                      routingKey: "email-queue",
                                      basicProperties: null,
                                      body: messageBody);
            }
        }

        public async Task<User?> GetUser(Guid id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task UpdateUser(Guid userId, User user)
        {
            // Retrieve the original user from the database
            var originalUser = await _userRepository.GetByIdAsync(userId);

            // Update the user's properties
            originalUser.FirstName = user.FirstName;
            originalUser.LastName = user.LastName;
            originalUser.Address = user.Address;
            originalUser.City = user.City;
            originalUser.PostalCode = user.PostalCode;
            originalUser.Country = user.Country;
            originalUser.Region = user.Region;
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

                return token;
            }

            return null;
        }

        public async Task ResetPassword(Guid userId, string newPassword)
        {
            var user = GetUser(userId).Result;

            if (user != null)
            {
                user.Password = newPassword;

                await UpdateUser(userId, user);
            }
        }

        public void SendResetToken(User user)
        {
            // Generate password reset token
            var token = GenerateResetJwtToken(user);

            // Create password reset link
            var resetLink = $"{_configuration["AppSettings:BaseUrl"]}/ofos/reset-password/{user.Id}/{token}";

            // Create email message
            var emailMessage = new EmailMessage(user.Email, "Password reset", $"Click this link to reset your password: {resetLink}");

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
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience = _configuration["JwtSettings:Audience"],
                Issuer = _configuration["JwtSettings:Issuer"]
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateResetJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim("ResetToken", Guid.NewGuid().ToString()) // Add a ResetToken claim to the JWT
                }),
                Expires = DateTime.UtcNow.AddMinutes(15), // Set expiration time to 15 minutes
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience = _configuration["JwtSettings:Audience"],
                Issuer = _configuration["JwtSettings:Issuer"]
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
