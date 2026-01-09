namespace Ona.Commit.Domain.Interfaces.Workers
{
    public interface IAppointmentReminderWorker
    {
        Task SendPendingRemindersAsync();
    }
}
