using Microsoft.AspNetCore.Http;
using Ona.Domain.Shared.Interfaces;

namespace Ona.Auth.Infrastructure.Services
{
    public class CurrentTenant : ICurrentTenant
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentTenant(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? Id => GetTenantId();

        public bool IsAvailable => Id.HasValue;

        private Guid? GetTenantId()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            if (context.Items.TryGetValue("TenantId", out var tenantIdObj) && tenantIdObj is Guid tenantId)
            {
                return tenantId;
            }

            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var tenantClaim = context.User.FindFirst("tenant");
                if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var claimTenantId))
                {
                    return claimTenantId;
                }
            }

            if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantHeader) &&
                Guid.TryParse(tenantHeader, out var headerTenantId))
            {
                return headerTenantId;
            }

            return null;
        }
    }
}
