using Ona.Commit.Application.DTOs.Requests;
using Ona.Commit.Application.DTOs.Responses;

namespace Ona.Commit.Application.Interfaces.Services
{
    public interface IProxyServerAppService
    {
        Task<ProxyServerDto> CreateAsync(CreateProxyServerRequest request);
        Task<IEnumerable<ProxyServerDto>> GetAllAsync();
        Task DeleteAsync(Guid id);
        Task ActivateAsync(Guid id);
        Task DeactivateAsync(Guid id);
    }
}
