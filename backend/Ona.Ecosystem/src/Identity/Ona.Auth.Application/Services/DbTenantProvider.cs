using Microsoft.Extensions.Caching.Memory;
using Ona.Auth.Application.Interfaces.Services;
using Ona.Core.Tenant;

namespace Ona.Auth.Application.Services
{
    public class DbTenantProvider : ITenantProvider
    {
        private readonly IMemoryCache _cache;
        private readonly ITenantService _tenantService;

        public DbTenantProvider(IMemoryCache cache, ITenantService tenantService)
        {
            _cache = cache;
            _tenantService = tenantService;
        }

        public async Task<TenantContext?> GetAsync(Guid tenantId)
        {
            return await _cache.GetOrCreateAsync($"tenant:{tenantId}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);

                var dto = await _tenantService.GetByIdAsync(tenantId);

                if (dto == null)
                    return null;

                return new TenantContext
                {
                    TenantId = dto.Id,
                    Name = dto.Name,
                    Domain = dto.Domain,
                    TimeZone = dto.TimeZone,
                    WhatsAppApiKey = dto.WhatsAppApiKey
                };
            });
        }

        public void Invalidate(Guid tenantId)
        {
            _cache.Remove($"tenant:{tenantId}");
        }
    }
}
