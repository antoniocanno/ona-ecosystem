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
            // Google sends these headers
            if (!Request.Headers.TryGetValue("X-Goog-Resource-ID", out var resourceId) ||
                !Request.Headers.TryGetValue("X-Goog-Channel-ID", out var channelId))
            {
                // Just return OK to acknowledge, even if data is missing, to avoid retries
                return Ok();
            }

            // Enqueue job
            _hangfireClient.Enqueue<ICalendarSyncWorker>(x => x.SyncFromGoogleAsync(resourceId.ToString(), channelId.ToString()));

            return Ok();
        }

        [HttpPost("outlook")]
        public async Task<IActionResult> OutlookWebhook([FromQuery] string? validationToken = null)
        {
            // Outlook handshake
            if (!string.IsNullOrEmpty(validationToken))
            {
                return Content(validationToken, "text/plain");
            }

            // Handle notifications
            // In a real implementation, we parse the JSON body to get the subscriptionId
            // and resource data. For now, we'll assume we can find the integration.

            // Enqueue job - the resourceId would come from the body['value'][0]['subscriptionId']
            // _hangfireClient.Enqueue<ICalendarSyncWorker>(x => x.SyncFromOutlookAsync(subscriptionId, ""));

            return Ok();
        }
    }
}
