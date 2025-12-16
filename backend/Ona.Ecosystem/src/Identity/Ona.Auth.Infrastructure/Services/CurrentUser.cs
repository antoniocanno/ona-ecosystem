using Microsoft.AspNetCore.Http;
using Ona.Domain.Shared.Interfaces;
using System.Security.Claims;

namespace Ona.Auth.Infrastructure.Services
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? Id
        {
            get
            {
                var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    userId = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

                return userId != null && Guid.TryParse(userId, out var id) ? id : null;
            }
        }
    }
}
