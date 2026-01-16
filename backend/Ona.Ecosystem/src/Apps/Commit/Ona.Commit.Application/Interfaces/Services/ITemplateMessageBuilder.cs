using Ona.Commit.Domain.Entities;

namespace Ona.Commit.Application.Interfaces.Services
{
    public interface ITemplateMessageBuilder
    {
        /// <summary>
        /// Monta o texto simples para mensagem via Evolution API usando templates customizados.
        /// Preenche as variáveis:
        /// {{1}} - Nome do Paciente (ou Nome da Clínica + Paciente se for conta compartilhada)
        /// {{2}} - Nome da Clínica/Tenant
        /// {{3}} - Data e Hora formatadas
        /// {{4}} - Link de confirmação/cancelamento
        /// </summary>
        /// <param name="appointment">Dados do agendamento.</param>
        /// <returns>Texto da mensagem com variáveis substituídas.</returns>
        Task<string> BuildTextReminderAsync(Appointment appointment);
    }
}
