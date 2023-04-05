using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OFOS.Domain.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using UserService.Core;

namespace UserService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConnection _rabbitConnection;
        private readonly IModel _rabbitChannel;

        public UserController(IUserService userService, IConnection rabbitConnection)
        {
            _userService = userService;
            _rabbitConnection = rabbitConnection;
            _rabbitChannel = _rabbitConnection.CreateModel();
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<User?>> GetUser(Guid id)
        {
            var user = await _userService.GetUser(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] User user)
        {
            if (id != user.Id)
                return BadRequest();

            await _userService.UpdateUser(user);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            await _userService.DeleteUser(id);

            return NoContent();
        }

        [HttpPost("create")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] User model)
        {
            if(model == null) return BadRequest();

            await _userService.CreateUser(model);

            return Ok("User: " + model.Id + " is created" );
        }

        [HttpPost("authenticate")]
        [AllowAnonymous]
        public async Task<ActionResult<string?>> Authenticate([FromBody] User model)
        {
            var token = await _userService.Authenticate(model.Email, model.Password);

            if (token == null)
                return Unauthorized();

            return Ok(token);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Email))
            {
                return BadRequest("Please provide a valid email address");
            }

            var user = await _userService.GetUserByEmail(model.Email);

            if (user == null)
            {
                // Do not reveal that the user does not exist
                return Ok();
            }

            // Generate password reset token
            var token =  _userService.GenerateJwtToken(user);

            //Update the user with the new retrievalToken
            user.RetrievalToken = token;
            await _userService.UpdateUser(user);

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

            return Ok();
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.NewPassword))
            {
                return BadRequest("Please provide valid credentials");
            }

            var user = await _userService.GetUserByEmail(model.Email);

            if (user == null || user.RetrievalToken != model.Token)
            {
                // Do not reveal that the user does not exist
                return Ok();
            }

            //Reset password with user, token and new password
            await _userService.ResetPassword(user, model.Token, model.NewPassword);

            return Ok();
        }

        public class ResetPasswordModel
        {
            public string? Email { get; set; }
            public string? Token { get; set; }
            public string? NewPassword { get; set; }
        }

        public class ForgotPasswordModel
        {
            public string? Email { get; set; }
        }


    }
}
