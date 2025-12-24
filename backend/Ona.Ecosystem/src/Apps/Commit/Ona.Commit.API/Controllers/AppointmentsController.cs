using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ona.Commit.Application.DTOs.Request;
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
        public async Task<IActionResult> GetAll()
        {
            var appointments = await _appointmentAppService.ListAsync();
            return Ok(appointments);
        }

        [HttpGet("{id:guid}")]
        [AuthorizeRoles(Role.Operator)]
        public async Task<IActionResult> GetById(Guid id)
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
