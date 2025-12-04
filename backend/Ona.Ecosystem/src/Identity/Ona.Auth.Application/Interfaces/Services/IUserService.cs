using Ona.Auth.Domain.Entities;

namespace Ona.Auth.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<ApplicationUser?> GetByIdAsync(string id);
        Task<ApplicationUser?> GetByEmailAsync(string email);
        Task<ApplicationUser?> GetByGoogleIdAsync(string googleId);
        Task<ApplicationUser> CreateAsync(ApplicationUser user);
        Task UpdateGoogleId(ApplicationUser user, string googleId);
        Task LockAsync(ApplicationUser user, DateTime lockoutEnd);
        Task UnlockAsync(ApplicationUser user);
        Task ConfirmEmailAsync(ApplicationUser user);
        Task SaveNewPasswordHashAsync(ApplicationUser user, string newPasswordHash);
        void ValidateUser(ApplicationUser? user);
    }
}
