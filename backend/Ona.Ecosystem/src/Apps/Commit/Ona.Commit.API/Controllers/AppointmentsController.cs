using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ona.Commit.Application.DTOs.Requests;
using Ona.Commit.Application.DTOs.Responses;
using Ona.Commit.Application.Interfaces.Services;
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

        public AppointmentsController(IAppointmentAppService appointmentAppService)
        {
            _appointmentAppService = appointmentAppService;
        }

        [HttpGet]
        [AuthorizeRoles(Role.Operator)]
        public async Task<IActionResult> List([FromQuery] DateTimeOffset startDate, [FromQuery] DateTimeOffset endDate, [FromQuery] Guid professionalId)
        {
            var appointments = await _appointmentAppService.ListAsync(startDate, endDate, professionalId);
            return Ok(appointments);
        }

        [HttpGet("{id:guid}")]
        [AuthorizeRoles(Role.Operator)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var appointment = await _appointmentAppService.GetByIdAsync(id);
            return Ok(appointment);
        }

        [HttpGet("{id:guid}/status")]
        [AuthorizeRoles(Role.Operator)]
        public async Task<IActionResult> GetStatus(Guid id)
        {
            var appointment = await _appointmentAppService.GetByIdAsync(id);
            return Ok(new AppointmentStatusDto { Status = appointment.Status });
        }

        [HttpPost]
        [AuthorizeRoles(Role.Operator)]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
        {
            var appointment = await _appointmentAppService.CreateAsync(request);
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
