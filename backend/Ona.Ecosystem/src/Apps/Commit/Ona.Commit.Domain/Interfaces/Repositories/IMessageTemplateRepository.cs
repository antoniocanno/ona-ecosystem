using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Enums;
using Ona.Core.Interfaces;

namespace Ona.Commit.Domain.Interfaces.Repositories
{
    public interface IMessageTemplateRepository : IBaseRepository<MessageTemplate>
    {
        Task<MessageTemplate?> GetByTypeAsync(Guid tenantId, NotificationType type);
    }
}
