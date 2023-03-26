using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OFOS.Domain.Models;
using UserService.Core;

namespace UserService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
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

        [HttpPost("authenticate")]
        [AllowAnonymous]
        public async Task<ActionResult<string?>> Authenticate([FromBody] User model)
        {
            var user = await _userService.Authenticate(model.Email, model.Password);

            if (user == null)
                return Unauthorized();

            return Ok(user);
        }
    }
}
