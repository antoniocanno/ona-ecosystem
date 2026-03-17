using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Core.Common.Enums;
using Ona.ServiceDefaults.Attributes;

namespace Ona.Commit.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AlertsController : ControllerBase
    {
        private readonly IAlertAppService _alertService;

        public AlertsController(IAlertAppService alertService)
        {
            _alertService = alertService;
        }

        [HttpGet("unread")]
        [AuthorizeRoles(Role.Operator)]
        public async Task<IActionResult> GetUnread()
        {
            var alerts = await _alertService.GetUnreadAsync();
            return Ok(alerts);
        }

        [HttpPost("{id:guid}/read")]
        [AuthorizeRoles(Role.Operator)]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            await _alertService.MarkAsReadAsync(id);
            return NoContent();
        }
    }
}
