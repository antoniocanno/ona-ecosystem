namespace Ona.Auth.API.Middlewares
{
    public class InternalApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public InternalApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("X-Internal-Api-Key", out var extractedApiKey))
            {
                var configuredApiKey = _configuration["Auth:InternalApiKey"];

                if (!string.IsNullOrEmpty(configuredApiKey) && configuredApiKey.Equals(extractedApiKey))
                {
                    var claims = new[]
                    {
                        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "InternalSystem"),
                        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "System"),
                        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin"),
                        new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Manager"),
                        new System.Security.Claims.Claim("IsInternal", "true")
                    };

                    var identity = new System.Security.Claims.ClaimsIdentity(claims, "InternalApiKey");
                    context.User = new System.Security.Claims.ClaimsPrincipal(identity);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
            }

            await _next(context);
        }
    }

}
