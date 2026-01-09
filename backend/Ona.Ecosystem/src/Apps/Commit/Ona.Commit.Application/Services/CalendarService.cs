using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Interfaces.Repositories;
using Ona.Core.Common.Exceptions;

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
            var integration = await GetActiveIntegrationAsync(appointment.ProfessionalId);
            if (integration == null) return;

            string externalId = integration.Provider switch
            {
                CalendarProvider.Google => await _googleCalendarService.CreateEventAsync(integration, appointment),
                CalendarProvider.Outlook => await _outlookCalendarService.CreateEventAsync(integration, appointment),
                _ => throw new ValidationException("Opção de calendário inválida.")
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

            var integration = await GetActiveIntegrationAsync(appointment.ProfessionalId);
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

            var integration = await GetActiveIntegrationAsync(appointment.ProfessionalId);
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

        public async Task SubscribeToNotificationsAsync(Guid professionalId)
        {
            var integration = await GetActiveIntegrationAsync(professionalId);
            if (integration == null) return;

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

            _integrationRepository.Update(integration);
            await _integrationRepository.SaveChangesAsync();
        }

        public async Task UnsubscribeFromNotificationsAsync(Guid professionalId, CalendarProvider provider)
        {
            var integration = await _integrationRepository.GetByProfessionalAndProviderAsync(professionalId, provider);
            if (integration == null) return;

            switch (integration.Provider)
            {
                case CalendarProvider.Google:
                    await _googleCalendarService.UnsubscribeFromNotificationsAsync(integration);
                    break;
                case CalendarProvider.Outlook:
                    await _outlookCalendarService.UnsubscribeFromNotificationsAsync(integration);
                    break;
            }
        }

        private async Task<CalendarIntegration?> GetActiveIntegrationAsync(Guid professionalId)
        {
            var integration = await _integrationRepository.GetByProfessionalAndProviderAsync(professionalId, CalendarProvider.Google);
            if (integration == null || !integration.IsActive)
            {
                integration = await _integrationRepository.GetByProfessionalAndProviderAsync(professionalId, CalendarProvider.Outlook);
            }

            return (integration != null && integration.IsActive) ? integration : null;
        }
    }
}
