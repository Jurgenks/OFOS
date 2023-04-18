using Microsoft.AspNetCore.Mvc;
using NotificationService.Core;
using OFOS.Domain.Models;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("listen")]
        public async Task<IActionResult> ListenToMessagesAsync()
        {
            await _notificationService.StartAsync();

            return Ok();
        }

        [HttpPost("email")]
        public IActionResult SendEmail([FromBody] EmailMessage email)
        {
            if (email.To != null && email.Subject != null && email.Body != null)
            {
                _notificationService.SendEmailMessage(email);
                return Ok();
            }
            else
            {
                return BadRequest(email);
            }

        }
    }
}
