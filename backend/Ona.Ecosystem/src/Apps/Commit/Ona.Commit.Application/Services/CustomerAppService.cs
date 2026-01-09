using Ona.Commit.Application.DTOs.Requests;
using Ona.Commit.Application.DTOs.Responses;
using Ona.Commit.Application.Interfaces.Repositories;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;
using Ona.Core.Common.Exceptions;
using Ona.Core.Interfaces;

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
            if (customer == null)
                throw new NotFoundException("Cliente não encontrado.");

            return customer;
        }

        public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request)
        {
            if (_currentUser.Id == Guid.Empty)
                throw new ValidationException("Contexto do usuário é obrigatório.");

            var customer = new Customer(
                _currentUser.Id.Value,
                request.Name,
                request.PhoneNumber,
                request.Email,
                request.InternalNotes);

            customer = await _repository.CreateAsync(customer);
            await _repository.SaveChangesAsync();

            return customer;
        }

        public async Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerRequest request)
        {
            var customer = await _repository.GetByIdAsync(id);
            if (customer == null)
                throw new NotFoundException("Cliente não encontrado.");

            if (request.Name != null)
                customer.UpdateName(request.Name);

            if (request.PhoneNumber != null)
                customer.UpdatePhoneNumber(request.PhoneNumber);

            if (request.Email != null)
                customer.UpdateEmail(request.Email);

            if (request.InternalNotes != null)
                customer.UpdateInternalNotes(request.InternalNotes);

            customer = _repository.Update(customer);
            await _repository.SaveChangesAsync();

            return customer;
        }

        public async Task DeleteAsync(Guid id)
        {
            var customer = await _repository.GetByIdAsync(id);
            if (customer == null)
                throw new NotFoundException("Cliente não encontrado.");

            customer.Delete();

            _repository.Update(customer);
            await _repository.SaveChangesAsync();
        }
    }
}
