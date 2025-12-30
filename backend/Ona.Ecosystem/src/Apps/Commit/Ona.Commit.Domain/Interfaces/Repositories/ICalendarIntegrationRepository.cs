using Ona.Commit.Domain.Entities;
using Ona.Core.Interfaces;

namespace Ona.Commit.Domain.Interfaces.Repositories
{
    public interface ICalendarIntegrationRepository : IBaseRepository<CalendarIntegration>
    {
        Task<CalendarIntegration?> GetByCustomerAndProviderAsync(Guid customerId, CalendarProvider provider);
        Task<CalendarIntegration?> GetByExternalResourceIdAsync(string resourceId);
    }
}
