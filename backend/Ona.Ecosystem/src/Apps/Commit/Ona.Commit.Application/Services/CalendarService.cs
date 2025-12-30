using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Interfaces.Repositories;

namespace Ona.Commit.Application.Services
{
    public class CalendarService : ICalendarService
    {
        private readonly ICalendarIntegrationRepository _integrationRepository;
        private readonly IExternalCalendarEventMappingRepository _mappingRepository;
        private readonly IGoogleCalendarService _googleCalendarService;
        private readonly IOutlookCalendarService _outlookCalendarService;

        public CalendarService(
            ICalendarIntegrationRepository integrationRepository,
            IExternalCalendarEventMappingRepository mappingRepository,
            IGoogleCalendarService googleCalendarService,
            IOutlookCalendarService outlookCalendarService)
        {
            _integrationRepository = integrationRepository;
            _mappingRepository = mappingRepository;
            _googleCalendarService = googleCalendarService;
            _outlookCalendarService = outlookCalendarService;
        }

        public async Task CreateAppointmentEventAsync(Appointment appointment)
        {
            var integration = await GetActiveIntegrationAsync(appointment.UserId);
            if (integration == null) return;

            string externalId = integration.Provider switch
            {
                CalendarProvider.Google => await _googleCalendarService.CreateEventAsync(integration, appointment),
                CalendarProvider.Outlook => await _outlookCalendarService.CreateEventAsync(integration, appointment),
                _ => throw new ArgumentOutOfRangeException()
            };

            var mapping = new ExternalCalendarEventMapping
            {
                AppointmentId = appointment.Id,
                ExternalEventId = externalId,
                LastSyncedAt = DateTimeOffset.UtcNow
            };
            mapping.SetTenantId(appointment.TenantId);

            await _mappingRepository.CreateAsync(mapping);
            await _mappingRepository.SaveChangesAsync();
        }

        public async Task UpdateAppointmentEventAsync(Appointment appointment)
        {
            var mapping = await _mappingRepository.GetByAppointmentIdAsync(appointment.Id);
            if (mapping == null) return;

            var integration = await GetActiveIntegrationAsync(appointment.UserId);
            if (integration == null) return;

            switch (integration.Provider)
            {
                case CalendarProvider.Google:
                    await _googleCalendarService.UpdateEventAsync(integration, appointment, mapping.ExternalEventId);
                    break;
                case CalendarProvider.Outlook:
                    await _outlookCalendarService.UpdateEventAsync(integration, appointment, mapping.ExternalEventId);
                    break;
            }

            mapping.LastSyncedAt = DateTimeOffset.UtcNow;
            _mappingRepository.Update(mapping);
            await _mappingRepository.SaveChangesAsync();
        }

        public async Task DeleteAppointmentEventAsync(Appointment appointment)
        {
            var mapping = await _mappingRepository.GetByAppointmentIdAsync(appointment.Id);
            if (mapping == null) return;

            var integration = await GetActiveIntegrationAsync(appointment.UserId);
            if (integration == null) return;

            switch (integration.Provider)
            {
                case CalendarProvider.Google:
                    await _googleCalendarService.DeleteEventAsync(integration, mapping.ExternalEventId);
                    break;
                case CalendarProvider.Outlook:
                    await _outlookCalendarService.DeleteEventAsync(integration, mapping.ExternalEventId);
                    break;
            }

            _mappingRepository.Remove(mapping);
            await _mappingRepository.SaveChangesAsync();
        }

        public async Task SubscribeToNotificationsAsync(Guid userId)
        {
            var integration = await GetActiveIntegrationAsync(userId);
            if (integration == null) return;

            // TODO: Retrieve specific webhook URL from configuration
            var webhookBase = "https://api.ona.com/api/v1/webhooks/calendar";

            switch (integration.Provider)
            {
                case CalendarProvider.Google:
                    await _googleCalendarService.SubscribeToNotificationsAsync(integration, $"{webhookBase}/google");
                    break;
                case CalendarProvider.Outlook:
                    await _outlookCalendarService.SubscribeToNotificationsAsync(integration, $"{webhookBase}/outlook");
                    break;
            }

            // Update integration with new ResourceId/ChannelId set by the service
            _integrationRepository.Update(integration);
            await _integrationRepository.SaveChangesAsync();
        }

        private async Task<CalendarIntegration?> GetActiveIntegrationAsync(Guid userId)
        {
            var integration = await _integrationRepository.GetByCustomerAndProviderAsync(userId, CalendarProvider.Google);
            if (integration == null || !integration.IsActive)
            {
                integration = await _integrationRepository.GetByCustomerAndProviderAsync(userId, CalendarProvider.Outlook);
            }

            return (integration != null && integration.IsActive) ? integration : null;
        }
    }
}
