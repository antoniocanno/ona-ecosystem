using Mapster;
using Microsoft.Extensions.Logging;
using Ona.Commit.Application.DTOs.Requests;
using Ona.Commit.Application.DTOs.Responses;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Interfaces.Repositories;

namespace Ona.Commit.Application.Services
{
    public class ProxyServerAppService : IProxyServerAppService
    {
        private readonly IProxyServerRepository _proxyRepository;
        private readonly ILogger<ProxyServerAppService> _logger;

        public ProxyServerAppService(
            IProxyServerRepository proxyRepository,
            ILogger<ProxyServerAppService> logger)
        {
            _proxyRepository = proxyRepository;
            _logger = logger;
        }

        public async Task<ProxyServerDto> CreateAsync(CreateProxyServerRequest request)
        {
            var proxy = new ProxyServer(
                request.Host,
                request.Port,
                request.Protocol,
                request.MaxTenants,
                request.Username,
                request.Password
            );

            await _proxyRepository.CreateAsync(proxy);
            await _proxyRepository.SaveChangesAsync();

            _logger.LogInformation("Proxy criado: {Id} - {Host}:{Port}", proxy.Id, proxy.Host, proxy.Port);

            return proxy;
        }

        public async Task<IEnumerable<ProxyServerDto>> GetAllAsync()
        {
            var proxies = await _proxyRepository.GetAllAsync(p => p.Tenants);
            return proxies.Adapt<IEnumerable<ProxyServerDto>>();
        }

        public async Task ActivateAsync(Guid id)
        {
            var proxy = await _proxyRepository.GetByIdAsync(id);
            if (proxy == null) throw new KeyNotFoundException($"Proxy {id} não encontrado.");

            proxy.Activate();
            _proxyRepository.Update(proxy);
            await _proxyRepository.SaveChangesAsync();
        }

        public async Task DeactivateAsync(Guid id)
        {
            var proxy = await _proxyRepository.GetByIdAsync(id);
            if (proxy == null) throw new KeyNotFoundException($"Proxy {id} não encontrado.");

            proxy.Deactivate();
            _proxyRepository.Update(proxy);
            await _proxyRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var proxy = await _proxyRepository.GetByIdAsync(id, p => p.Tenants);
            if (proxy == null) throw new KeyNotFoundException($"Proxy {id} não encontrado.");

            if (proxy.Tenants.Any())
            {
                throw new InvalidOperationException("Não é possível deletar um proxy que possui tenants vinculados. Migre os tenants primeiro.");
            }

            _proxyRepository.Remove(proxy);
            await _proxyRepository.SaveChangesAsync();
        }
    }
}
