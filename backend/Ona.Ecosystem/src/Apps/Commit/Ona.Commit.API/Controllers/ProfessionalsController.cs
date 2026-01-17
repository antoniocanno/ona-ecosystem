using Microsoft.AspNetCore.Mvc;
using Ona.Commit.Application.DTOs.Requests;
using Ona.Commit.Application.DTOs.Responses;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Core.Common.Enums;
using Ona.ServiceDefaults.Attributes;

namespace Ona.Commit.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ProfessionalsController : ControllerBase
    {
        private readonly IProfessionalAppService _service;

        public ProfessionalsController(IProfessionalAppService service)
        {
            _service = service;
        }

        [HttpGet]
        [AuthorizeRoles(Role.Operator)]
        public async Task<ActionResult<IEnumerable<ProfessionalDto>>> List()
        {
            var result = await _service.ListAsync();
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [AuthorizeRoles(Role.Operator)]
        public async Task<ActionResult<ProfessionalDto>> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("me")]
        public async Task<ActionResult<ProfessionalDto>> GetMe()
        {
            var result = await _service.GetByCurrentUserIdAsync();
            if (result == null) return NotFound("Perfil de profissional não encontrado para o usuário logado.");
            return Ok(result);
        }

        [HttpPost]
        [AuthorizeRoles(Role.Manager)]
        public async Task<ActionResult<ProfessionalDto>> Create([FromBody] CreateProfessionalRequest request)
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPost("register")]
        [AuthorizeRoles(Role.Manager)]
        public async Task<ActionResult<ProfessionalDto>> Register([FromBody] RegisterProfessionalRequest request)
        {
            var result = await _service.RegisterProfessionalWithUserAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:guid}")]
        [AuthorizeRoles(Role.Operator)]
        public async Task<ActionResult<ProfessionalDto>> Update(Guid id, [FromBody] UpdateProfessionalRequest request)
        {
            var result = await _service.UpdateAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [AuthorizeRoles(Role.Manager)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
