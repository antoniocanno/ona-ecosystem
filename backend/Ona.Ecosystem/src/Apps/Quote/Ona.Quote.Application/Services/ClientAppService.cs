using Mapster;
using Ona.Domain.Shared.Interfaces;
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
        private readonly ICurrentUser _currentUser;

        public ClientAppService(IClientRepository repository, ICurrentUser currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<ClientDto> CreateAsync(ClientCreateRequest request)
        {
            if (!_currentUser.Id.HasValue)
                throw new Exception("Contexto de usuário necessário");

            var userId = _currentUser.Id.Value;
            var client = new Client(userId, request.Name, request.Email, request.Phone);
            await _repository.CreateAsync(client);
            return client.Adapt<ClientDto>();
        }

        public async Task<ClientDto> UpdateAsync(ClientUpdateRequest request)
        {
            if (!_currentUser.Id.HasValue)
                throw new Exception("Contexto de usuário necessário");

            var userId = _currentUser.Id.Value;
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
