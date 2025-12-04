using Ona.Auth.API.Attributes;

namespace Ona.Auth.API.Middleware
{
    public class EmailVerificationMiddleware
    {
        private readonly RequestDelegate _next;

        public EmailVerificationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            if (IsAllowedWithoutVerification(context))
            {
                await _next(context);
                return;
            }

            var emailVerified = context.User.Claims
                .FirstOrDefault(c => c.Type == "email_verified")?.Value;

            if (!bool.TryParse(emailVerified, out bool isVerified) || !isVerified)
            {
                await Results.Problem(
                    title: "Verificação de email necessária",
                    statusCode: StatusCodes.Status403Forbidden,
                    type: "https://ona.com/errors/email-not-verified",
                    detail: "Verifique seu email para acessar esta funcionalidade.",
                    instance: context.Request.Path
                ).ExecuteAsync(context);
                return;
            }

            await _next(context);
        }

        private static bool IsAllowedWithoutVerification(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            return endpoint?.Metadata.GetMetadata<AllowWithoutEmailVerificationAttribute>() != null;
        }
    }
}
