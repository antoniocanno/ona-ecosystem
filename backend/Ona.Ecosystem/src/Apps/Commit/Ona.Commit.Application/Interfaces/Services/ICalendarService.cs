using Ona.Commit.Domain.Entities;

namespace Ona.Commit.Application.Interfaces.Services
{
    public interface ICalendarService
    {
        Task CreateAppointmentEventAsync(Appointment appointment);
        Task UpdateAppointmentEventAsync(Appointment appointment);
        Task DeleteAppointmentEventAsync(Appointment appointment);
        Task SubscribeToNotificationsAsync(Guid userId);
    }
}
