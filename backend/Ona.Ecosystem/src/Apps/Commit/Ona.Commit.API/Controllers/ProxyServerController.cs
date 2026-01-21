using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ona.Commit.Application.DTOs.Requests;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Core.Common.Enums;
using Ona.ServiceDefaults.Attributes;

namespace Ona.Commit.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProxyServerController : ControllerBase
    {
        private readonly IProxyServerAppService _service;

        public ProxyServerController(IProxyServerAppService service)
        {
            _service = service;
        }

        [HttpPost]
        [AuthorizeRoles(Role.Admin)]
        public async Task<IActionResult> Create([FromBody] CreateProxyServerRequest request)
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
        }

        [HttpGet]
        [AuthorizeRoles(Role.Admin)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpPost("{id:guid}/activate")]
        [AuthorizeRoles(Role.Admin)]
        public async Task<IActionResult> Activate(Guid id)
        {
            await _service.ActivateAsync(id);
            return Ok(new { message = "Proxy ativado com sucesso" });
        }

        [HttpPost("{id:guid}/deactivate")]
        [AuthorizeRoles(Role.Admin)]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            await _service.DeactivateAsync(id);
            return Ok(new { message = "Proxy desativado com sucesso" });
        }

        [HttpDelete("{id:guid}")]
        [AuthorizeRoles(Role.Admin)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
