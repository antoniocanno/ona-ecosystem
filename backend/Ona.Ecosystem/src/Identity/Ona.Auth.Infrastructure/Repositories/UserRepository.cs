using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ona.Auth.Application.Interfaces.Repositories;
using Ona.Auth.Domain.Entities;
using Ona.Auth.Infrastructure.Data;
using Ona.Infrastructure.Shared.Repositories;

namespace Ona.Auth.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<AuthDbContext, ApplicationUser>, IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IUserTenantRoleRepository _userTenantRoleRepository;

        public UserRepository(
            AuthDbContext authDbContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IUserTenantRoleRepository userTenantRoleRepository)
            : base(authDbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userTenantRoleRepository = userTenantRoleRepository;
        }

        public async Task<IEnumerable<ApplicationUser>> ListAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userTenantRoleRepository.GetAllAsync();

            var roleMap = userRoles
                .Join(
                    roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { ur.UserId, RoleName = r.Name }
                )
                .GroupBy(x => x.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.RoleName).ToList()
                );

            foreach (var user in users)
            {
                if (roleMap.TryGetValue(user.Id, out var userRolesList))
                    user.Roles = userRolesList;
                else
                    user.Roles = [];
            }

            return users;
        }


        public async Task<ApplicationUser?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<ApplicationUser?> GetByGoogleIdAsync(string googleId)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.GoogleId == googleId);
        }

        public async Task<ApplicationUser?> GetByIdAsync(string id)
        {
            return await _dbSet.FindAsync(id);
        }
    }
}
