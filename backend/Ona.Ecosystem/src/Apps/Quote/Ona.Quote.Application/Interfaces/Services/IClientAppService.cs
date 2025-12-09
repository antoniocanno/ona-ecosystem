using Ona.Quote.Application.DTOs.Request;
using Ona.Quote.Application.DTOs.Response;

namespace Ona.Quote.Application.Interfaces.Services
{
    public interface IClientAppService
    {
        Task<ClientDto> CreateAsync(Guid userId, ClientCreateRequest request);
        Task<ClientDto> UpdateAsync(Guid userId, ClientUpdateRequest request);
    }
}
