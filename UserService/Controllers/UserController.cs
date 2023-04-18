using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OFOS.Domain.Models;
using System.Security.Claims;
using UserService.Core;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

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
            // Get the user id from the JWT
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Check if the user making the request is the same as the user whose data is being retrieved
            if (userId != id.ToString())
                return Forbid();

            var user = await _userService.GetUser(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] User user)
        {
            // Get the user id from the JWT
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Check if the user making the request is the same as the user whose data is being retrieved
            if (userId != id.ToString())
                return Forbid();

            if (id != user.Id)
                return BadRequest();

            await _userService.UpdateUser(user);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            // Get the user id from the JWT
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Check if the user making the request is the same as the user whose data is being retrieved
            if (userId != id.ToString())
                return Forbid();

            await _userService.DeleteUser(id);

            return NoContent();
        }

        [HttpPost("create")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] User model)
        {
            if (model == null) return BadRequest();

            var user = await _userService.GetUserByEmail(model.Email);

            if (user == null)
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

            // Create a new cookie and set the value to the JWT token
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.UtcNow.AddDays(7), // Set the cookie expiration time
            };

            Response.Cookies.Append("jwt", token, cookieOptions);

            return Ok(token);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Please provide a valid email address");
            }

            //Get User by E-mail address
            var user = await _userService.GetUserByEmail(email);

            if (user == null)
            {
                // Do not reveal that the user does not exist
                return Ok();
            }

            //Send ResetToken to UserService
            _userService.SendResetToken(user);

            return Ok();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword))
            {
                return BadRequest("Please provide a new password");
            }

            // Get the user id from the JWT
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return BadRequest("No ResetToken provided");
            }

            //Reset password with user, token and new password
            await _userService.ResetPassword(Guid.Parse(userId), newPassword);

            return Ok();
        }

        public class ResetPasswordModel
        {
            public string? NewPassword { get; set; }
        }

        public class ForgotPasswordModel
        {
            public string? Email { get; set; }
        }


    }
}
