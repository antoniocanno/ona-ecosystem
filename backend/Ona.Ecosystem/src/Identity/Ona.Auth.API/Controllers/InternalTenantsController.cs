using Microsoft.AspNetCore.Mvc;
using Ona.Auth.Application.Interfaces.Services;
using Ona.Core.Tenant;

namespace Ona.Auth.API.Controllers
{
    [Route("api/internal/tenants")]
    [ApiController]
    public class InternalTenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;

        public InternalTenantsController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetContext(Guid id)
        {
            var dto = await _tenantService.GetByIdAsync(id);
            if (dto == null) return NotFound();

            var context = new TenantContext
            {
                TenantId = dto.Id,
                Name = dto.Name,
                Domain = dto.Domain,
                TimeZone = dto.TimeZone,
                WhatsAppApiKey = dto.WhatsAppApiKey
            };

            return Ok(context);
        }
    }
}
