using Ona.Commit.Domain.Entities;
using Ona.Core.Interfaces;

namespace Ona.Commit.Domain.Interfaces.Repositories
{
    public interface IExternalCalendarEventMappingRepository : IBaseRepository<ExternalCalendarEventMapping>
    {
        Task<ExternalCalendarEventMapping?> GetByAppointmentIdAsync(Guid appointmentId);
        Task<ExternalCalendarEventMapping?> GetByExternalEventIdAsync(string externalEventId);
    }
}
