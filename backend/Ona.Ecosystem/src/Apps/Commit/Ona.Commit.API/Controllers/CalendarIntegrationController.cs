using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ona.Commit.Application.DTOs.Responses;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;

namespace Ona.Commit.API.Controllers
{
    [ApiController]
    [Route("api/v1/integrations/calendar/oauth")]
    public class CalendarIntegrationController : ControllerBase
    {
        private readonly ICalendarIntegrationService _service;

        public CalendarIntegrationController(ICalendarIntegrationService service)
        {
            _service = service;
        }

        [HttpPost("initiate")]
        [Authorize]
        public IActionResult Initiate([FromBody] InitiateCalendarAuthRequest request)
        {
            var url = _service.GetAuthUrl(request);
            return Ok(new { Url = url });
        }

        [HttpPost("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback([FromBody] CompleteCalendarAuthRequest request)
        {
            var result = await _service.CompleteAuthAsync(request);
            return Ok(result);
        }

        [HttpDelete("{professionalId}/{provider}")]
        [Authorize]
        public async Task<IActionResult> Remove(Guid professionalId, CalendarProvider provider)
        {
            await _service.RemoveIntegrationAsync(professionalId, provider);
            return NoContent();
        }
    }
}
