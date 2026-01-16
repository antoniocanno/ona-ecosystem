using Microsoft.AspNetCore.Http;
using Ona.Core.Tenant;
using Ona.ServiceDefaults.Attributes;

namespace Ona.ServiceDefaults.Middlewares
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ITenantContextAccessor accessor,
            ITenantProvider tenantProvider)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var tenantClaim = context.User.FindFirst("tenant");
                if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var tenantId))
                {
                    context.Items["TenantId"] = tenantId;

                    var endpoint = context.GetEndpoint();
                    if (endpoint?.Metadata?.GetMetadata<SkipTenantValidationAttribute>() != null)
                    {
                        await _next(context);
                        return;
                    }

                    var tenantContext = await tenantProvider.GetAsync(tenantId);
                    accessor.SetCurrent(tenantContext);
                }
            }

            await _next(context);
        }
    }
}
