using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OFOS.Domain.Models;
using System.Security.Claims;
using UserService.Core;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace UserService.Controllers
{
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
        [Authorize(Roles = "Customer, Employee")]
        public async Task<ActionResult<User?>> GetUser([FromRoute] Guid id)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != id.ToString())
                return Forbid();

            var user = await _userService.GetUser(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpGet("email")]
        public async Task<ActionResult<User?>> GetUserByEmail([FromQuery] string email)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return BadRequest("No authentication is found.");

            var user = await _userService.GetUserByEmail(email);

            if (user == null)
                return NotFound();

            if (userId != user.Id.ToString())
                return Forbid();

            return Ok(user);
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateUser([FromBody] User user)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != user.Id.ToString())
                return Forbid();

            await _userService.UpdateUser(Guid.Parse(userId), user);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid id)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != id.ToString())
                return Forbid();

            await _userService.DeleteUser(id);

            return NoContent();
        }

        [HttpPost("create")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] User model)
        {
            if (model == null)
                return BadRequest();

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
        public async Task<IActionResult> Authenticate([FromForm] string email, [FromForm] string password)
        {
            var token = await _userService.Authenticate(email, password);

            if (token == null)
                return Unauthorized();

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
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

            var user = await _userService.GetUserByEmail(email);

            if (user == null)
            {
                return Ok(); // Do not reveal that the user does not exist
            }

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

            try
            {
                var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                await _userService.ResetPassword(Guid.Parse(userId), newPassword);

                return Ok();
            }
            catch
            {
                return BadRequest("No ResetToken provided");
            }
        }

        [HttpGet("employee/tasks")]
        [Authorize(Roles = "Employee")]
        public IActionResult GetTasks()
        {
            return Ok("Get back to work!");
        }
    }
}
