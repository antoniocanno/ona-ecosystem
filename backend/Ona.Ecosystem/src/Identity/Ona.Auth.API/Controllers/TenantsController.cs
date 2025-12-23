using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ona.Auth.Application.DTOs.Request;
using Ona.Auth.Application.Interfaces.Services;
using Ona.Core.Common.Enums;
using Ona.ServiceDefaults.Attributes;

namespace Ona.Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;

        public TenantsController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
        {
            var tenant = await _tenantService.CreateTenantAsync(request);
            return Ok(new { tenant.Id, tenant.Name, tenant.Domain });
        }

        [HttpGet("{id:guid}")]
        [AuthorizeRoles(Role.Manager)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var tenant = await _tenantService.GetByIdAsync(id);
            if (tenant == null) return NotFound();
            return Ok(tenant);
        }

        [HttpGet]
        [AuthorizeRoles(Role.Manager)]
        public async Task<IActionResult> List()
        {
            var tenants = await _tenantService.ListAsync();
            return Ok(tenants);
        }

        [HttpPut("{id:guid}")]
        [AuthorizeRoles(Role.Manager)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantRequest request)
        {
            var tenant = await _tenantService.UpdateAsync(id, request);
            return Ok(tenant);
        }

        [HttpDelete("{id:guid}")]
        [AuthorizeRoles(Role.Manager)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _tenantService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{id:guid}/suspend")]
        [AuthorizeRoles(Role.Manager)]
        public async Task<IActionResult> Suspend(Guid id)
        {
            await _tenantService.SuspendAsync(id);
            return NoContent();
        }

        [HttpPost("{id:guid}/activate")]
        [AuthorizeRoles(Role.Manager)]
        public async Task<IActionResult> Activate(Guid id)
        {
            await _tenantService.ActivateAsync(id);
            return NoContent();
        }
    }
}
