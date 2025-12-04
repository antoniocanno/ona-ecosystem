using Ona.Auth.Domain.Entities;

namespace Ona.Auth.Application.Interfaces.Services
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(ApplicationUser user);
    }
}
