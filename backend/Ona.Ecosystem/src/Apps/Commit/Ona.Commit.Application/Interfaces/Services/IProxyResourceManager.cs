using Ona.Commit.Domain.Entities;

namespace Ona.Commit.Application.Interfaces.Services
{
    public interface IProxyResourceManager
    {
        /// <summary>
        /// Aloca um proxy disponível para o tenant. 
        /// Se o tenant já tiver um proxy, retorna o atual.
        /// </summary>
        Task<ProxyServer?> AllocateProxyAsync(Guid tenantId);

        /// <summary>
        /// Libera o proxy associado ao tenant (se houver).
        /// </summary>
        Task ReleaseProxyAsync(Guid tenantId);

        /// <summary>
        /// Marca o proxy atual como falho (para este tenant) e tenta alocar um novo.
        /// </summary>
        Task<ProxyServer?> RotateProxyAsync(Guid tenantId);
    }
}
