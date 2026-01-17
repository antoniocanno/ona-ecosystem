using Ona.Commit.Domain.Entities;
using Ona.Core.Interfaces;

namespace Ona.Commit.Domain.Interfaces.Repositories
{
    public interface IMessageInteractionLogRepository : IBaseRepository<MessageInteractionLog>
    {
        Task<MessageInteractionLog?> GetByExternalMessageIdAsync(string externalMessageId);

        /// <summary>
        /// Verifica se existe alguma mensagem enviada para ou recebida deste cliente nas últimas 24 horas.
        /// </summary>
        /// <param name="tenantId">ID do Tenant.</param>
        /// <param name="customerId">ID do Cliente vinculado aos agendamentos.</param>
        /// <returns>Verdadeiro se houver interação recente.</returns>
        Task<bool> HasRecentInteractionAsync(Guid tenantId, Guid customerId);
        Task<int> CountMessagesSentTodayAsync(Guid tenantId);
    }
}
