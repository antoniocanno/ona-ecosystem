using Microsoft.AspNetCore.Http;
using Ona.Domain.Shared.Interfaces;
using System.Security.Claims;

namespace Ona.ServiceDefaults.Services
{
    /// <summary>
    /// Implementação padrão de ICurrentUser que extrai informações do usuário do HttpContext claims.
    /// Pode ser usada por qualquer projeto API que use autenticação JWT.
    /// </summary>
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public Guid? Id
        {
            get
            {
                var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    userId = User?.FindFirst("sub")?.Value;

                return userId != null && Guid.TryParse(userId, out var id) ? id : null;
            }
        }

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

        public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value;

        public bool IsEmailVerified
        {
            get
            {
                var emailVerifiedClaim = User?.FindFirst("email_verified")?.Value;
                return emailVerifiedClaim != null
                    && bool.TryParse(emailVerifiedClaim, out bool isVerified)
                    && isVerified;
            }
        }

        public string AuthMethod => User?.FindFirst("auth_method")?.Value ?? "unknown";
    }
}
