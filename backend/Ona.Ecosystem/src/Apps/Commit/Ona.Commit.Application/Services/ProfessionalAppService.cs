using Ona.Commit.Application.DTOs.Requests;
using Ona.Commit.Application.DTOs.Responses;
using Ona.Commit.Application.Interfaces.Repositories;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;
using Ona.Core.Common.Exceptions;
using Ona.Core.Interfaces;

namespace Ona.Commit.Application.Services
{
    public class ProfessionalAppService : IProfessionalAppService
    {
        private readonly IProfessionalRepository _repository;
        private readonly ICurrentUser _currentUser;
        private readonly IIdentityService _identityService;

        public ProfessionalAppService(
            IProfessionalRepository repository,
            ICurrentUser currentUser,
            IIdentityService identityService)
        {
            _repository = repository;
            _currentUser = currentUser;
            _identityService = identityService;
        }

        public async Task<IEnumerable<ProfessionalDto>> ListAsync()
        {
            var professionals = await _repository.GetAllAsync();
            return professionals.Select(p => (ProfessionalDto)p);
        }

        public async Task<ProfessionalDto?> GetByIdAsync(Guid id)
        {
            var professional = await _repository.GetByIdAsync(id);
            if (professional == null) throw new NotFoundException("Profissional não encontrado.");
            return professional;
        }

        public async Task<ProfessionalDto?> GetByEmailAsync(string email)
        {
            var professional = await _repository.GetByEmailAsync(email);
            if (professional == null) return null;
            return professional;
        }

        public async Task<ProfessionalDto?> GetByCurrentUserIdAsync()
        {
            if (!_currentUser.Id.HasValue || _currentUser.Id.Value == Guid.Empty) return null;
            var professional = await _repository.GetByApplicationUserIdAsync(_currentUser.Id.Value);
            if (professional == null) return null;
            return professional;
        }

        public async Task<ProfessionalDto> CreateAsync(CreateProfessionalRequest request)
        {
            var existing = await _repository.GetByEmailAsync(request.Email);
            if (existing != null) throw new ValidationException("Já existe um profissional com este e-mail.");

            var professional = new Professional(
                request.Name,
                request.Email,
                request.PhoneNumber,
                request.ApplicationUserId
            );

            await _repository.CreateAsync(professional);
            await _repository.SaveChangesAsync();

            return professional;
        }

        public async Task<ProfessionalDto> RegisterProfessionalWithUserAsync(RegisterProfessionalRequest request)
        {
            var applicationUserId = await _identityService.CreateUserAsync(
                request.Email,
                request.Name,
                request.Password,
                request.Role
            );

            var professional = new Professional(
                request.Name,
                request.Email,
                request.PhoneNumber,
                applicationUserId
            );

            await _repository.CreateAsync(professional);
            await _repository.SaveChangesAsync();

            return professional;
        }

        public async Task<ProfessionalDto> UpdateAsync(Guid id, UpdateProfessionalRequest request)
        {
            var professional = await _repository.GetByIdAsync(id);
            if (professional == null) throw new NotFoundException("Profissional não encontrado.");

            if (!string.IsNullOrEmpty(request.Name)) professional.SetName(request.Name);
            if (!string.IsNullOrEmpty(request.Email)) professional.SetEmail(request.Email);
            if (!string.IsNullOrEmpty(request.PhoneNumber)) professional.SetPhoneNumber(request.PhoneNumber);
            if (request.ApplicationUserId.HasValue) professional.SetApplicationUserId(request.ApplicationUserId.Value);

            _repository.Update(professional);
            await _repository.SaveChangesAsync();

            return professional;
        }

        public async Task DeleteAsync(Guid id)
        {
            var professional = await _repository.GetByIdAsync(id);
            if (professional == null) throw new NotFoundException("Profissional não encontrado.");

            _repository.Remove(professional);
            await _repository.SaveChangesAsync();
        }
    }
}
