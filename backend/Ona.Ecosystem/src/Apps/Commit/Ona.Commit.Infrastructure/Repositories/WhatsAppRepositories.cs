using Microsoft.EntityFrameworkCore;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Interfaces.Repositories;
using Ona.Commit.Infrastructure.Data;
using Ona.Infrastructure.Shared.Repositories;

namespace Ona.Commit.Infrastructure.Repositories
{
    public class TenantWhatsAppConfigRepository : BaseRepository<CommitDbContext, TenantWhatsAppConfig>, ITenantWhatsAppConfigRepository
    {
        public TenantWhatsAppConfigRepository(CommitDbContext context) : base(context) { }

        public async Task<TenantWhatsAppConfig?> GetByTenantIdAsync(Guid tenantId)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.TenantId == tenantId);
        }
    }

    public class WhatsAppTemplateRegistryRepository : BaseRepository<CommitDbContext, WhatsAppTemplateRegistry>, IWhatsAppTemplateRegistryRepository
    {
        public WhatsAppTemplateRegistryRepository(CommitDbContext context) : base(context) { }

        public async Task<WhatsAppTemplateRegistry?> GetByLogicalNameAsync(Guid tenantId, string logicalName)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.LogicalName == logicalName);
        }
    }

    public class MessageInteractionLogRepository : BaseRepository<CommitDbContext, MessageInteractionLog>, IMessageInteractionLogRepository
    {
        public MessageInteractionLogRepository(CommitDbContext context) : base(context) { }

        public async Task<MessageInteractionLog?> GetByExternalMessageIdAsync(string externalMessageId)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.ExternalMessageId == externalMessageId);
        }

        public async Task<bool> HasRecentInteractionAsync(Guid tenantId, Guid customerId)
        {
            var yesterday = DateTimeOffset.UtcNow.AddHours(-24);

            return await _dbSet
                .Include(log => log.Appointment)
                .AnyAsync(log =>
                    log.TenantId == tenantId &&
                    log.CreatedAt >= yesterday &&
                    log.Appointment != null &&
                    log.Appointment.CustomerId == customerId);
        }

        public async Task<int> CountMessagesSentTodayAsync(Guid tenantId)
        {
            var today = DateTimeOffset.UtcNow.Date;
            return await _dbSet.CountAsync(x => x.TenantId == tenantId && x.CreatedAt >= today);
        }
    }

    public class WhatsAppNumberVerificationRepository : BaseRepository<CommitDbContext, WhatsAppNumberVerification>, IWhatsAppNumberVerificationRepository
    {
        public WhatsAppNumberVerificationRepository(CommitDbContext context) : base(context) { }

        public async Task<WhatsAppNumberVerification?> GetByPhoneNumberAsync(string phoneNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);
        }
    }
}
