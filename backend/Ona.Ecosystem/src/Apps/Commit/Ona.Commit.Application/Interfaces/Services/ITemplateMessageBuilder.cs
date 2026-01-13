using Ona.Commit.Domain.Entities;

namespace Ona.Commit.Application.Interfaces.Services
{
    public interface ITemplateMessageBuilder
    {
        /// <summary>
        /// Monta o payload JSON para o template de lembrete de agendamento (WhatsApp/Meta).
        /// Preenche as variáveis:
        /// {{1}} - Nome do Paciente (ou Nome da Clínica + Paciente se for conta compartilhada)
        /// {{2}} - Nome da Clínica/Tenant
        /// {{3}} - Data e Hora formatadas
        /// {{4}} - Link de confirmação/cancelamento
        /// </summary>
        /// <param name="appointment">Dados do agendamento com Customer e Professional carregados.</param>
        /// <returns>Objeto contendo o payload serializado ou estrutura de dados para o envio.</returns>
        Task<string> BuildReminderPayloadAsync(Appointment appointment);
    }
}
