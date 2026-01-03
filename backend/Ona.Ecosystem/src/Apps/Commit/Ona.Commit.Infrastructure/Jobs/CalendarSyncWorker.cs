using Ona.Commit.Application.Interfaces.Repositories;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Enums;
using Ona.Commit.Domain.Interfaces.Repositories;
using Ona.Commit.Domain.Interfaces.Workers;

namespace Ona.Commit.Infrastructure.Jobs
{
    public class CalendarSyncWorker : ICalendarSyncWorker
    {
        private readonly ICalendarIntegrationRepository _integrationRepo;
        private readonly IExternalCalendarEventMappingRepository _mappingRepo;
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly IGoogleCalendarService _googleService;
        private readonly IOutlookCalendarService _outlookService;

        public CalendarSyncWorker(
            ICalendarIntegrationRepository integrationRepo,
            IExternalCalendarEventMappingRepository mappingRepo,
            IAppointmentRepository appointmentRepo,
            IGoogleCalendarService googleService,
            IOutlookCalendarService outlookService)
        {
            _integrationRepo = integrationRepo;
            _mappingRepo = mappingRepo;
            _appointmentRepo = appointmentRepo;
            _googleService = googleService;
            _outlookService = outlookService;
        }

        public async Task SyncFromGoogleAsync(string resourceId, string channelId)
        {
            var integration = await _integrationRepo.GetByExternalResourceIdAsync(resourceId);
            if (integration == null) return;

            await _googleService.GetValidAccessTokenAsync(integration);
            _integrationRepo.Update(integration);
            await _integrationRepo.SaveChangesAsync();

            var changedEvents = await _googleService.GetChangedEventsAsync(integration);

            foreach (var extEvent in changedEvents)
            {
                var mapping = await _mappingRepo.GetByExternalEventIdAsync(extEvent.Id);
                if (mapping == null) continue;

                if (extEvent.Updated <= mapping.LastSyncedAt) continue;

                var appointment = await _appointmentRepo.GetByIdAsync(mapping.AppointmentId);
                if (appointment == null) continue;

                if (extEvent.Status == "cancelled")
                {
                    appointment.UpdateStatus(AppointmentStatus.Canceled);
                }
                else
                {
                    if (appointment.StartDate != extEvent.Start || appointment.EndDate != extEvent.End)
                    {
                        appointment.Reschedule(extEvent.Start.UtcDateTime, extEvent.End.UtcDateTime);
                    }
                }

                _appointmentRepo.Update(appointment);
                await _appointmentRepo.SaveChangesAsync();

                mapping.LastSyncedAt = DateTimeOffset.UtcNow;
                _mappingRepo.Update(mapping);
                await _mappingRepo.SaveChangesAsync();
            }
        }

        public async Task SyncFromOutlookAsync(string resourceId, string subscriptionId)
        {
            var integration = await _integrationRepo.GetByExternalResourceIdAsync(resourceId);
            if (integration == null) return;

            await _outlookService.GetValidAccessTokenAsync(integration);
            _integrationRepo.Update(integration);
            await _integrationRepo.SaveChangesAsync();

            var changedEvents = await _outlookService.GetChangedEventsAsync(integration);

            foreach (var extEvent in changedEvents)
            {
                var mapping = await _mappingRepo.GetByExternalEventIdAsync(extEvent.Id);
                if (mapping == null) continue;

                if (extEvent.Updated <= mapping.LastSyncedAt) continue;

                var appointment = await _appointmentRepo.GetByIdAsync(mapping.AppointmentId);
                if (appointment == null) continue;

                if (extEvent.Status == "cancelled")
                {
                    appointment.UpdateStatus(AppointmentStatus.Canceled);
                }
                else
                {
                    if (appointment.StartDate != extEvent.Start || appointment.EndDate != extEvent.End)
                    {
                        appointment.Reschedule(extEvent.Start.UtcDateTime, extEvent.End.UtcDateTime);
                    }
                }

                _appointmentRepo.Update(appointment);
                await _appointmentRepo.SaveChangesAsync();

                mapping.LastSyncedAt = DateTimeOffset.UtcNow;
                _mappingRepo.Update(mapping);
                await _mappingRepo.SaveChangesAsync();
            }
        }
    }
}
