using Ona.Commit.Application.Interfaces.Repositories;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Enums;
using Ona.Commit.Domain.Interfaces.Repositories;
using Ona.Commit.Domain.Interfaces.Workers;

namespace Ona.Commit.Worker.Hangfire.Jobs
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
            // 1. Find integration by resourceId
            var integration = await _integrationRepo.GetByExternalResourceIdAsync(resourceId);
            if (integration == null) return;

            // Ensure token is fresh
            await _googleService.GetValidAccessTokenAsync(integration);
            _integrationRepo.Update(integration);
            await _integrationRepo.SaveChangesAsync();

            // 2. Fetch changed events
            var changedEvents = await _googleService.GetChangedEventsAsync(integration);

            foreach (var extEvent in changedEvents)
            {
                var mapping = await _mappingRepo.GetByExternalEventIdAsync(extEvent.Id);
                if (mapping == null) continue; // Event not known to us

                // 3. Reconciliation Logic (Avoid echo loop)
                // If the external event update time is older or equal to our last sync, ignore
                if (extEvent.Updated <= mapping.LastSyncedAt) continue;

                var appointment = await _appointmentRepo.GetByIdAsync(mapping.AppointmentId);
                if (appointment == null) continue;

                // 4. Update Local Appointment
                if (extEvent.Status == "cancelled")
                {
                    appointment.UpdateStatus(AppointmentStatus.Canceled);
                }
                else
                {
                    // Check if date/time changed
                    if (appointment.StartDate != extEvent.Start || appointment.EndDate != extEvent.End)
                    {
                        appointment.Reschedule(extEvent.Start.UtcDateTime, extEvent.End.UtcDateTime);
                    }
                }

                _appointmentRepo.Update(appointment);
                await _appointmentRepo.SaveChangesAsync();

                // 5. Update Mapping to prevent loop
                mapping.LastSyncedAt = DateTimeOffset.UtcNow; // This ensures that if we send back an update, we know it was us
                _mappingRepo.Update(mapping);
                await _mappingRepo.SaveChangesAsync();
            }
        }

        public async Task SyncFromOutlookAsync(string resourceId, string subscriptionId)
        {
            // 1. Find integration by resourceId (or subscriptionId)
            var integration = await _integrationRepo.GetByExternalResourceIdAsync(resourceId);
            if (integration == null) return;

            // Ensure token is fresh
            await _outlookService.GetValidAccessTokenAsync(integration);
            _integrationRepo.Update(integration);
            await _integrationRepo.SaveChangesAsync();

            // 2. Fetch changed events
            var changedEvents = await _outlookService.GetChangedEventsAsync(integration);

            foreach (var extEvent in changedEvents)
            {
                var mapping = await _mappingRepo.GetByExternalEventIdAsync(extEvent.Id);
                if (mapping == null) continue;

                // 3. Reconciliation Logic
                if (extEvent.Updated <= mapping.LastSyncedAt) continue;

                var appointment = await _appointmentRepo.GetByIdAsync(mapping.AppointmentId);
                if (appointment == null) continue;

                // 4. Update Local Appointment
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

                // 5. Update Mapping
                mapping.LastSyncedAt = DateTimeOffset.UtcNow;
                _mappingRepo.Update(mapping);
                await _mappingRepo.SaveChangesAsync();
            }
        }
    }
}
