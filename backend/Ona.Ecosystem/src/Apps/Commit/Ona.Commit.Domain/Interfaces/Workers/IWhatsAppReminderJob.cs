namespace Ona.Commit.Domain.Interfaces.Workers
{
    public interface IWhatsAppReminderJob
    {
        /// <summary>
        /// Job Fire-and-Forget para processar e enviar um lembrete individual.
        /// </summary>
        /// <param name="appointmentId">ID do agendamento.</param>
        Task ProcessAndSendReminderAsync(Guid appointmentId);
    }
}
