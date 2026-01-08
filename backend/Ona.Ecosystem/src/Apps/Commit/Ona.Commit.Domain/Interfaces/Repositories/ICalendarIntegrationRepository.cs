using Ona.Commit.Domain.Entities;
using Ona.Core.Interfaces;

namespace Ona.Commit.Domain.Interfaces.Repositories
{
    public interface ICalendarIntegrationRepository : IBaseRepository<CalendarIntegration>
    {
        Task<CalendarIntegration?> GetByProfessionalAndProviderAsync(Guid professionalId, CalendarProvider provider);
        Task<CalendarIntegration?> GetByExternalResourceIdAsync(string resourceId);
    }
}
