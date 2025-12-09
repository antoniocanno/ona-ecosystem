using Microsoft.AspNetCore.Http;
using Ona.Commit.Application.Interfaces;

namespace Ona.Commit.Infrastructure.MultiTenancy
{
    public class HttpContextTenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HttpContextTenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid TenantId
        {
            get
            {
                var ctx = _httpContextAccessor.HttpContext;
                if (ctx == null) return Guid.Empty;
                if (ctx.Items.TryGetValue("TenantId", out var v) && v is Guid g) return g;
                return Guid.Empty;
            }
        }

        public bool HasTenant => TenantId != Guid.Empty;
    }
}
