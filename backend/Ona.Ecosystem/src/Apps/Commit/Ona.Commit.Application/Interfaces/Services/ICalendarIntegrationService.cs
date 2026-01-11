using Ona.Commit.Application.DTOs.Responses;
using Ona.Commit.Domain.Entities;

namespace Ona.Commit.Application.Interfaces.Services
{
    public interface ICalendarIntegrationService
    {
        string GetAuthUrl(InitiateCalendarAuthRequest request);
        Task<CalendarIntegrationResponse> CompleteAuthAsync(CompleteCalendarAuthRequest request);
        Task RemoveIntegrationAsync(Guid professionalId, CalendarProvider provider);
        Task<IEnumerable<ExternalEventDto>> ListExternalEventsAsync(Guid professionalId, CalendarProvider provider);
    }
}
