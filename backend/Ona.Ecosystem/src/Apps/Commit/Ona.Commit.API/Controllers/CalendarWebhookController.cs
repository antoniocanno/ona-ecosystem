using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ona.Commit.Domain.Interfaces.Workers;

namespace Ona.Commit.API.Controllers
{
    [ApiController]
    [Route("api/v1/webhooks/calendar")]
    [AllowAnonymous]
    public class CalendarWebhookController : ControllerBase
    {
        private readonly IBackgroundJobClient _hangfireClient;

        public CalendarWebhookController(IBackgroundJobClient hangfireClient)
        {
            _hangfireClient = hangfireClient;
        }

        [HttpPost("google")]
        public IActionResult GoogleWebhook()
        {
            if (!Request.Headers.TryGetValue("X-Goog-Resource-ID", out var resourceId) ||
                !Request.Headers.TryGetValue("X-Goog-Channel-ID", out var channelId))
            {
                return Ok();
            }

            _hangfireClient.Enqueue<ICalendarSyncWorker>(x => x.SyncFromGoogleAsync(resourceId.ToString(), channelId.ToString()));

            return Ok();
        }
    }
}
