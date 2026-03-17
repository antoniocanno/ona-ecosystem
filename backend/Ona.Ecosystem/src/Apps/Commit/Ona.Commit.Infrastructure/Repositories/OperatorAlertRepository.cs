using Microsoft.EntityFrameworkCore;
using Ona.Commit.Application.Interfaces.Repositories;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Infrastructure.Data;
using Ona.Infrastructure.Shared.Repositories;

namespace Ona.Commit.Infrastructure.Repositories
{
    public class OperatorAlertRepository : BaseRepository<CommitDbContext, OperatorAlert>, IOperatorAlertRepository
    {
        public OperatorAlertRepository(CommitDbContext context) : base(context) { }

        public async Task<IEnumerable<OperatorAlert>> GetUnreadAsync()
        {
            return await _dbSet
                .Where(x => !x.IsRead)
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .ToListAsync();
        }
    }
}
