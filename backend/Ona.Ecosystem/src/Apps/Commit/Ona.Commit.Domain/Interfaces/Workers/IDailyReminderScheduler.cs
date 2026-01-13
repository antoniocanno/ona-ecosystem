namespace Ona.Commit.Domain.Interfaces.Workers
{
    public interface IDailyReminderScheduler
    {
        /// <summary>
        /// Busca agendamentos para o dia seguinte e enfileira jobs individuais de lembrete.
        /// </summary>
        Task ScheduleDailyRemindersAsync();
    }
}
