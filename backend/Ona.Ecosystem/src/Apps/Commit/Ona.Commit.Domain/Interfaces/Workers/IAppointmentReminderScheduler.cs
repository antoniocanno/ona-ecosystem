namespace Ona.Commit.Domain.Interfaces.Workers
{
    public interface IAppointmentReminderScheduler
    {
        /// <summary>
        /// Busca agendamentos próximos e enfileira jobs de lembrete.
        /// </summary>
        Task ScheduleRemindersAsync();
    }
}
