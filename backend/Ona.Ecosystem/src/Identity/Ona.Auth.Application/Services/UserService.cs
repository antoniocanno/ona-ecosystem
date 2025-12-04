using Mapster;
using Ona.Auth.Application.DTOs.Request;
using Ona.Auth.Application.DTOs.Responses;
using Ona.Auth.Application.Interfaces.Repositories;
using Ona.Auth.Application.Interfaces.Services;
using Ona.Auth.Domain.Entities;
using Ona.Auth.Domain.Exceptions;

namespace Ona.Auth.Application.Services
{
    public class UserService : IUserService, IUserAppService
    {
        private readonly IUserRepository _repository;

        public UserService(
            IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApplicationUser?> GetByIdAsync(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<ApplicationUser?> GetByEmailAsync(string email)
        {
            return await _repository.GetByEmailAsync(email);
        }

        public async Task<ApplicationUser?> GetByGoogleIdAsync(string googleId)
        {
            return await _repository.GetByGoogleIdAsync(googleId);
        }

        public async Task<ApplicationUser> CreateAsync(ApplicationUser user)
        {
            var createdUser = await _repository.CreateAsync(user);
            await _repository.SaveChangesAsync();
            return createdUser;
        }

        public async Task UpdateGoogleId(ApplicationUser user, string googleId)
        {
            user.GoogleId = googleId;
            user.UpdatedAt = DateTime.UtcNow;
            _repository.Update(user);
            await _repository.SaveChangesAsync();
        }

        public async Task SaveNewPasswordHashAsync(ApplicationUser user, string newPasswordHash)
        {
            user.PasswordHash = newPasswordHash;
            user.UpdatedAt = DateTime.UtcNow;
            _repository.Update(user);
            await _repository.SaveChangesAsync();
        }

        public async Task LockAsync(ApplicationUser user, DateTime lockoutEnd)
        {
            user.Lock(lockoutEnd);
            await _repository.SaveChangesAsync();
        }

        public async Task UnlockAsync(ApplicationUser user)
        {
            user.Unlock();
            await _repository.SaveChangesAsync();
        }

        public async Task ConfirmEmailAsync(ApplicationUser user)
        {
            user.MarkEmailAsVerified();
            _repository.Update(user);
            await _repository.SaveChangesAsync();
        }

        public void ValidateUser(ApplicationUser? user)
        {
            if (user == null)
                throw new ValidationException("Usuário não encontrado.");
        }

        #region App Services

        public async Task<UserDto> GetDtoByIdAsync(string id)
        {
            var user = await _repository.GetByIdAsync(id);
            ValidateUser(user);
            return user!.Adapt<UserDto>();
        }

        public async Task<UserDto> UpdateAsync(string id, UserUpdateRequest request)
        {
            var user = await _repository.GetByIdAsync(id);

            ValidateUser(user);

            if (!string.IsNullOrEmpty(request.Name))
            {
                user!.FullName = request.Name;
                user.UpdatedAt = DateTime.UtcNow;
            }

            user = _repository.Update(user!);
            await _repository.SaveChangesAsync();

            return user.Adapt<UserDto>();
        }

        #endregion
    }
}
