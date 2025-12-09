namespace Ona.Commit.API.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        public TenantMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var user = context.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var tenantClaim = user.FindFirst("tenant");
                if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var tenantId))
                {
                    context.Items["TenantId"] = tenantId;
                }
                else
                {
                    // opcional: logar ou falhar cedo dependendo da política
                }
            }

            await _next(context);
        }
    }
}
