using Ona.Commit.Application.DTOs.Responses;
using Ona.Commit.Domain.Entities;

namespace Ona.Commit.Application.Interfaces.Services
{
    public interface IOutlookCalendarService
    {
        string GetAuthUrl();
        Task<CalendarIntegration> ExchangeCodeForTokenAsync(string code, string state);
        Task<string> CreateEventAsync(CalendarIntegration integration, Appointment appointment);
        Task UpdateEventAsync(CalendarIntegration integration, Appointment appointment, string externalEventId);
        Task DeleteEventAsync(CalendarIntegration integration, string externalEventId);
        Task<string> GetValidAccessTokenAsync(CalendarIntegration integration);
        Task<bool> RefreshTokenIfNeededAsync(CalendarIntegration integration);
        Task<IEnumerable<ExternalEventDto>> GetEventsAsync(CalendarIntegration integration);

        // Webhook / Sync methods
        Task SubscribeToNotificationsAsync(CalendarIntegration integration, string webhookUrl);
        Task UnsubscribeFromNotificationsAsync(CalendarIntegration integration);
        Task<IEnumerable<ExternalEventDto>> GetChangedEventsAsync(CalendarIntegration integration);
    }
}
