using Microsoft.EntityFrameworkCore;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Enums;
using Ona.Commit.Domain.Interfaces.Repositories;
using Ona.Commit.Infrastructure.Data;
using Ona.Infrastructure.Shared.Repositories;

namespace Ona.Commit.Infrastructure.Repositories
{
    public class MessageTemplateRepository : BaseRepository<CommitDbContext, MessageTemplate>, IMessageTemplateRepository
    {
        public MessageTemplateRepository(CommitDbContext context) : base(context)
        {
        }

        public async Task<MessageTemplate?> GetByTypeAsync(Guid tenantId, NotificationType type)
        {
            return await _context.Set<MessageTemplate>()
                .Where(m => m.TenantId == tenantId && m.Type == type && m.IsActive)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}
