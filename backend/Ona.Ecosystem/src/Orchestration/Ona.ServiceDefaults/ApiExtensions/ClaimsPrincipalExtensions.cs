using System.Security;
using System.Security.Claims;

namespace Ona.ServiceDefaults.ApiExtensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            var idUsuarioClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idUsuarioClaim))
            {
                throw new SecurityException("Token inválido ou ID de usuário ausente.");
            }
            return idUsuarioClaim;
        }

        public static bool IsEmailVerified(this ClaimsPrincipal principal)
        {
            var emailVerifiedClaim = principal.FindFirst("email_verified")?.Value;
            return emailVerifiedClaim != null && bool.TryParse(emailVerifiedClaim, out bool isVerified) && isVerified;
        }

        public static string GetAuthMethod(this ClaimsPrincipal principal)
        {
            var authMethodClaim = principal.FindFirst("auth_method")?.Value;
            return authMethodClaim ?? "unknown";
        }
    }
}
