using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OFOS.Domain.Models;
using UserService.Core;

namespace UserService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
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
            if (model == null) return BadRequest();

            var user = await _userService.GetUserByEmail(model.Email);

            if(user == null)
            {
                await _userService.CreateUser(model);
                return Ok("User: " + model.Id + " is created");
            }
            else
            {
                return BadRequest("User: " + model.Email + " is already created");
            }


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

            //Get User by E-mail address
            var user = await _userService.GetUserByEmail(model.Email);

            if (user == null)
            {
                // Do not reveal that the user does not exist
                return Ok();
            }

            //Send ResetToken to UserService
            await _userService.SendResetToken(user);

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
