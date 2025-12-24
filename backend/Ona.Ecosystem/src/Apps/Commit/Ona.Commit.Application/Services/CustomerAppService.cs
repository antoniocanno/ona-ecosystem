using Ona.Commit.Application.DTOs;
using Ona.Commit.Application.DTOs.Request;
using Ona.Commit.Application.Interfaces.Repositories;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;
using Ona.Core.Common.Exceptions;
using Ona.Domain.Shared.Interfaces;

namespace Ona.Commit.Application.Services
{
    public class CustomerAppService : ICustomerAppService
    {
        private readonly ICurrentUser _currentUser;
        private readonly ICustomerRepository _repository;

        public CustomerAppService(
            ICurrentUser currentUser,
            ICustomerRepository repository)
        {
            _currentUser = currentUser;
            _repository = repository;
        }

        public async Task<IEnumerable<CustomerDto>> ListAsync()
        {
            var customers = await _repository.GetAllAsync();
            return customers.Select(c => (CustomerDto)c);
        }

        public async Task<CustomerDto?> GetByIdAsync(Guid id)
        {
            var customer = await _repository.GetByIdAsync(id);
            if (customer == null) return null;

            return customer;
        }

        public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request)
        {
            if (!_currentUser.Id.HasValue)
                throw new ValidationException("Contexto do usuário é obrigatório.");

            var customer = new Customer
            {
                UserId = _currentUser.Id.Value,
                Name = request.Name,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                InternalNotes = request.InternalNotes
            };

            return await _repository.CreateAsync(customer);
        }

        public async Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerRequest request)
        {
            var customer = await _repository.GetByIdAsync(id);
            if (customer == null)
                throw new NotFoundException("Cliente não encontrado.");

            if (request.Name != null) customer.Name = request.Name;
            if (request.PhoneNumber != null) customer.PhoneNumber = request.PhoneNumber;
            if (request.Email != null) customer.Email = request.Email;
            if (request.InternalNotes != null) customer.InternalNotes = request.InternalNotes;
            if (request.TotalNoShows.HasValue) customer.TotalNoShows = request.TotalNoShows.Value;

            customer.UpdatedAt = DateTimeOffset.UtcNow;
            customer = _repository.Update(customer);
            await _repository.SaveChangesAsync();

            return customer;
        }

        public async Task DeleteAsync(Guid id)
        {
            var customer = await _repository.GetByIdAsync(id);
            if (customer == null)
                throw new NotFoundException("Cliente não encontrado.");

            customer.IsDeleted = true;
            customer.UpdatedAt = DateTimeOffset.UtcNow;
            _repository.Update(customer);
            await _repository.SaveChangesAsync();
        }
    }
}
