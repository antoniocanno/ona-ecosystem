using Microsoft.EntityFrameworkCore;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Interfaces.Repositories;
using Ona.Commit.Infrastructure.Data;
using Ona.Infrastructure.Shared.Repositories;

namespace Ona.Commit.Infrastructure.Repositories
{
    public class ExternalCalendarEventMappingRepository : BaseRepository<CommitDbContext, ExternalCalendarEventMapping>, IExternalCalendarEventMappingRepository
    {
        public ExternalCalendarEventMappingRepository(CommitDbContext context) : base(context)
        {
        }

        public async Task<ExternalCalendarEventMapping?> GetByAppointmentIdAsync(Guid appointmentId)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);
        }

        public async Task<ExternalCalendarEventMapping?> GetByExternalEventIdAsync(string externalEventId)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.ExternalEventId == externalEventId);
        }
    }
}
