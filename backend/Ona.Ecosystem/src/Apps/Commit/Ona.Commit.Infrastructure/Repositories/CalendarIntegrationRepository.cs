using Microsoft.EntityFrameworkCore;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Interfaces.Repositories;
using Ona.Commit.Infrastructure.Data;
using Ona.Infrastructure.Shared.Repositories;

namespace Ona.Commit.Infrastructure.Repositories
{
    public class CalendarIntegrationRepository : BaseRepository<CommitDbContext, CalendarIntegration>, ICalendarIntegrationRepository
    {
        public CalendarIntegrationRepository(CommitDbContext context) : base(context)
        {
        }

        public async Task<CalendarIntegration?> GetByCustomerAndProviderAsync(Guid customerId, CalendarProvider provider)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.CustomerId == customerId && x.Provider == provider && x.IsActive);
        }

        public async Task<CalendarIntegration?> GetByExternalResourceIdAsync(string resourceId)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.ExternalResourceId == resourceId && x.IsActive);
        }
    }
}
