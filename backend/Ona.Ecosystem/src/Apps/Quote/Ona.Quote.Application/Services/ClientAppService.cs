using Mapster;
using Ona.Quote.Application.DTOs.Request;
using Ona.Quote.Application.DTOs.Response;
using Ona.Quote.Application.Interfaces.Services;
using Ona.Quote.Domain.Entities;
using Ona.Quote.Domain.Interfaces.Repositories;

namespace Ona.Quote.Application.Services
{
    public class ClientAppService : IClientAppService
    {
        private readonly IClientRepository _repository;

        public ClientAppService(IClientRepository repository)
        {
            _repository = repository;
        }

        public async Task<ClientDto> CreateAsync(Guid userId, ClientCreateRequest request)
        {
            var client = new Client(userId, request.Name, request.Email, request.Phone);
            await _repository.CreateAsync(client);
            return client.Adapt<ClientDto>();
        }

        public async Task<ClientDto> UpdateAsync(Guid userId, ClientUpdateRequest request)
        {
            var client = await _repository.GetByIdAsync(userId, request.Id);

            if (!string.IsNullOrEmpty(request.Name))
                client.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Email))
                client.Email = request.Email;
            if (!string.IsNullOrEmpty(request.Phone))
                client.Phone = request.Phone;

            _repository.Update(client);
            await _repository.SaveChangesAsync();
            return client.Adapt<ClientDto>();
        }
    }
}
