using Ona.Auth.Domain.Entities;

namespace Ona.Auth.Application.Interfaces.Repositories
{
    public interface IUserRepository : IBaseRepository<ApplicationUser>
    {
        Task<ApplicationUser?> GetByIdAsync(string id);
        Task<ApplicationUser?> GetByEmailAsync(string email);
        Task<ApplicationUser?> GetByGoogleIdAsync(string googleId);
    }
}
