using Ona.Auth.Domain.Entities;

namespace Ona.Auth.Domain.Interfaces.Services
{
    public interface IUserDomainService
    {
        Task<ApplicationUser> CreateUserAsync(ApplicationUser user, string? password = null);
        Task<ApplicationUser> CreateGoogleUserAsync(string email, string name, string googleId, bool emailVerified);
        Task ValidateUserForLoginAsync(ApplicationUser? user);
    }
}
