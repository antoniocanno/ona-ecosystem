using Microsoft.EntityFrameworkCore;
using Ona.Auth.Application.Interfaces.Repositories;
using Ona.Auth.Domain.Entities;
using Ona.Auth.Infrastructure.Data;

namespace Ona.Auth.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<ApplicationUser>, IUserRepository
    {
        public UserRepository(AuthDbContext authDbContext)
            : base(authDbContext) { }

        public async Task<ApplicationUser?> GetByEmailAsync(string email)
        {
            return await DbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<ApplicationUser?> GetByGoogleIdAsync(string googleId)
        {
            return await DbSet.FirstOrDefaultAsync(u => u.GoogleId == googleId);
        }

        public async Task<ApplicationUser?> GetByIdAsync(string id)
        {
            return await DbSet.FindAsync(id);
        }
    }
}
