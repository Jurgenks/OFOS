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

        [HttpPost("email")]
        public IActionResult SendEmail([FromBody] EmailMessage email)
        {
            if(email.To!= null && email.Subject!= null && email.Body != null)
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
