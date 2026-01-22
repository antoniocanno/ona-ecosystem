using Microsoft.AspNetCore.Mvc;
using Ona.Commit.Application.Interfaces.Services;

namespace Ona.Commit.API.Controllers
{
    [ApiController]
    [Route("api/alerts")]
    public class AlertsController : ControllerBase
    {
        private readonly IAlertAppService _alertAppService;

        public AlertsController(IAlertAppService alertAppService)
        {
            _alertAppService = alertAppService;
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread()
        {
            var alerts = await _alertAppService.GetUnreadAsync();
            return Ok(alerts);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            await _alertAppService.MarkAsReadAsync(id);
            return NoContent();
        }
    }
}
