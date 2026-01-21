using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Interfaces.Repositories;

namespace Ona.Commit.Infrastructure.Services
{
    public class ProxyResourceManager : IProxyResourceManager
    {
        private readonly IProxyServerRepository _proxyRepository;
        private readonly ITenantWhatsAppConfigRepository _configRepository;

        public ProxyResourceManager(
            IProxyServerRepository proxyRepository,
            ITenantWhatsAppConfigRepository configRepository)
        {
            _proxyRepository = proxyRepository;
            _configRepository = configRepository;
        }

        public async Task<ProxyServer?> AllocateProxyAsync(Guid tenantId)
        {
            var existingProxy = await _proxyRepository.GetByTenantIdAsync(tenantId);
            if (existingProxy != null)
            {
                return existingProxy;
            }

            var availableProxy = await _proxyRepository.GetAvailableProxyAsync();
            if (availableProxy == null)
            {
                return null;
            }

            var config = await _configRepository.GetOrCreateByTenantIdAsync(tenantId);
            config.ProxyServerId = availableProxy.Id;
            _configRepository.Update(config);
            await _configRepository.SaveChangesAsync();

            return availableProxy;
        }

        public async Task ReleaseProxyAsync(Guid tenantId)
        {
            var config = await _configRepository.GetByTenantIdAsync(tenantId);
            if (config != null && config.ProxyServerId != null)
            {
                config.ProxyServerId = null;
                _configRepository.Update(config);
                await _configRepository.SaveChangesAsync();
            }
        }

        public async Task<ProxyServer?> RotateProxyAsync(Guid tenantId)
        {
            var currentProxy = await _proxyRepository.GetByTenantIdAsync(tenantId);

            if (currentProxy != null)
            {
                // TODO: Isso afetará todos os tenants neste proxy. Idealmente teríamos um job para migrar os outros.
                currentProxy.Deactivate();
                _proxyRepository.Update(currentProxy);
                await _proxyRepository.SaveChangesAsync();

                await ReleaseProxyAsync(tenantId);
            }

            return await AllocateProxyAsync(tenantId);
        }
    }
}
