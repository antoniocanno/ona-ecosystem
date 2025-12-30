using Ona.Commit.Application.DTOs;

namespace Ona.Commit.Application.Interfaces.Services
{
    public interface ICalendarIntegrationService
    {
        string GetAuthUrl(InitiateCalendarAuthRequest request);
        Task<CalendarIntegrationResponse> CompleteAuthAsync(CompleteCalendarAuthRequest request);
    }
}
