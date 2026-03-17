using Microsoft.EntityFrameworkCore;
using Ona.Commit.Application.Interfaces.Repositories;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Infrastructure.Data;
using Ona.Infrastructure.Shared.Repositories;

namespace Ona.Commit.Infrastructure.Repositories
{
    public class ProfessionalRepository : BaseRepository<CommitDbContext, Professional>, IProfessionalRepository
    {
        public ProfessionalRepository(CommitDbContext context) : base(context)
        {
        }

        public async Task<Professional?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Email == email);
        }

        public async Task<Professional?> GetByApplicationUserIdAsync(Guid applicationUserId)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.ApplicationUserId == applicationUserId);
        }
    }
}
