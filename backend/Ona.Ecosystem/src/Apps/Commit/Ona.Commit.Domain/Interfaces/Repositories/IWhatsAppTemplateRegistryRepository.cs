using Ona.Commit.Domain.Entities;
using Ona.Core.Interfaces;

namespace Ona.Commit.Domain.Interfaces.Repositories
{
    public interface IWhatsAppTemplateRegistryRepository : IBaseRepository<WhatsAppTemplateRegistry>
    {
        Task<WhatsAppTemplateRegistry?> GetByLogicalNameAsync(Guid tenantId, string logicalName);
    }
}
