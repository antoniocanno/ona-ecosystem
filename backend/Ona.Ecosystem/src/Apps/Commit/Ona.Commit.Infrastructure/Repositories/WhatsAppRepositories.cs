using Microsoft.EntityFrameworkCore;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Enums;
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

        public async Task<TenantWhatsAppConfig?> GetByInstanceNameAsync(string instanceName)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.InstanceName == instanceName);
        }

        public async Task<TenantWhatsAppConfig> GetOrCreateByTenantIdAsync(Guid tenantId)
        {
            var config = await GetByTenantIdAsync(tenantId);
            if (config == null)
            {
                config = new TenantWhatsAppConfig(tenantId, WhatsAppProvider.None);
                await CreateAsync(config);
                await SaveChangesAsync();
            }
            return config;
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
            var now = DateTimeOffset.UtcNow;
            var today = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
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

    public class ProxyServerRepository : BaseRepository<CommitDbContext, ProxyServer>, IProxyServerRepository
    {
        public ProxyServerRepository(CommitDbContext context) : base(context) { }

        public async Task<ProxyServer?> GetAvailableProxyAsync()
        {
            return await _dbSet
                .Include(p => p.Tenants)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Tenants.Count)
                .FirstOrDefaultAsync(p => p.Tenants.Count < p.MaxTenants);
        }

        public async Task<ProxyServer?> GetByTenantIdAsync(Guid tenantId)
        {
            return await _dbSet
                .Include(p => p.Tenants)
                .FirstOrDefaultAsync(p => p.Tenants.Any(t => t.TenantId == tenantId));
        }
    }
}
