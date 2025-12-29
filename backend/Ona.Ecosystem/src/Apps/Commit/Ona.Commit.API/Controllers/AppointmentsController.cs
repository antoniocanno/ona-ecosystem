using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ona.Commit.Application.DTOs.Request;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Worker.Hangfire.Jobs;
using Ona.Core.Common.Enums;
using Ona.ServiceDefaults.Attributes;

namespace Ona.Commit.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentAppService _appointmentAppService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public AppointmentsController(IAppointmentAppService appointmentAppService, IBackgroundJobClient backgroundJobClient)
        {
            _appointmentAppService = appointmentAppService;
            _backgroundJobClient = backgroundJobClient;
        }

        [HttpGet("{id:guid}/status")]
        [AuthorizeRoles(Role.Operator)]
        public async Task<IActionResult> GetStatus(Guid id)
        {
            var appointment = await _appointmentAppService.GetByIdAsync(id);
            if (appointment == null)
                return NotFound();
            return Ok(appointment);
        }

        [HttpPost]
        [AuthorizeRoles(Role.Operator)]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
        {
            var appointment = await _appointmentAppService.CreateAsync(request);

            _backgroundJobClient.Enqueue<SendReminderJob>(job => job.Execute(appointment.Id));

            return Ok(appointment);
        }


        [HttpPatch("{id:guid}")]
        [AuthorizeRoles(Role.Operator)]
        public async Task<IActionResult> Update(Guid id, [FromBody] AppointmentUpdateRequest request)
        {
            var appointment = await _appointmentAppService.UpdateAsync(id, request);
            return Ok(appointment);
        }

        [HttpDelete("{id:guid}")]
        [AuthorizeRoles(Role.Operator)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _appointmentAppService.DeleteAsync(id);
            return NoContent();
        }
    }
}
