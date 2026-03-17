using Mapster;
using Microsoft.AspNetCore.Identity;
using Ona.Auth.Application.DTOs.Requests;
using Ona.Auth.Application.DTOs.Responses;
using Ona.Auth.Application.Interfaces.Repositories;
using Ona.Auth.Application.Interfaces.Services;
using Ona.Auth.Domain.Constants;
using Ona.Auth.Domain.Entities;
using Ona.Auth.Domain.Interfaces.Services;
using Ona.Core.Common.Exceptions;
using Ona.Core.Interfaces;

namespace Ona.Auth.Application.Services
{
    public class UserAppService : IUserAppService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IUserRepository _repository;
        private readonly ITenantInviteRepository _tenantInviteRepository;
        private readonly IUserTenantRoleRepository _userTenantRoleRepository;
        private readonly IEmailService _emailService;
        private readonly ICurrentTenant _currentTenant;
        private readonly ICurrentUser _currentUser;
        private readonly IUserDomainService _userDomainService;

        public UserAppService(
            IUserRepository repository,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ITenantInviteRepository tenantInviteRepository,
            IUserTenantRoleRepository userTenantRoleRepository,
            IEmailService emailService,
            ICurrentTenant currentTenant,
            ICurrentUser currentUser,
            IUserDomainService userDomainService)
        {
            _repository = repository;
            _userManager = userManager;
            _roleManager = roleManager;
            _tenantInviteRepository = tenantInviteRepository;
            _userTenantRoleRepository = userTenantRoleRepository;
            _emailService = emailService;
            _currentTenant = currentTenant;
            _currentUser = currentUser;
            _userDomainService = userDomainService;
        }

        private static void ValidateUser(ApplicationUser? user)
        {
            if (user == null)
                throw new ValidationException(AuthConstants.Errors.UserNotFound);
        }

        public async Task<IEnumerable<UserDto>> ListAsync()
        {
            var users = await _repository.ListAsync();
            return users.Adapt<IEnumerable<UserDto>>();
        }

        public async Task<UserDto> GetByIdAsync(Guid id)
        {
            var user = await _repository.GetByIdAsync(id);
            return user!;
        }

        public async Task<UserDto> UpdateAsync(Guid id, UserUpdateRequest request)
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

            return user;
        }

        public async Task<UserDto> GetMeAsync()
        {
            if (!_currentUser.Id.HasValue)
                throw new ValidationException(AuthConstants.Errors.UserContextRequired);

            return await GetByIdAsync(_currentUser.Id.Value);
        }

        public async Task<UserDto> UpdateMeAsync(UserUpdateRequest request)
        {
            if (!_currentUser.Id.HasValue)
                throw new ValidationException(AuthConstants.Errors.UserContextRequired);

            return await UpdateAsync(_currentUser.Id.Value, request);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
        {
            if (!_currentTenant.Id.HasValue)
                throw new ValidationException(AuthConstants.Errors.TenantContextRequired);

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                TenantId = _currentTenant.Id.Value,
                EmailConfirmed = true
            };
            user.MarkEmailAsVerified();

            user = await _userDomainService.CreateUserAsync(user, request.Password);

            if (string.IsNullOrEmpty(request.Role))
                throw new ValidationException(AuthConstants.Errors.RoleRequired);

            var role = await _roleManager.FindByNameAsync(request.Role);
            if (role == null) throw new NotFoundException(string.Format(AuthConstants.Errors.RoleNotFound, request.Role));

            await _userTenantRoleRepository.CreateAsync(new UserTenantRole(user.Id, role.Id, _currentTenant.Id.Value));
            await _userTenantRoleRepository.SaveChangesAsync();

            return user;
        }

        public async Task InviteUserAsync(InviteUserRequest request)
        {
            if (!_currentTenant.Id.HasValue)
                throw new ValidationException(AuthConstants.Errors.TenantContextRequired);

            if (!_currentUser.Id.HasValue)
                throw new ValidationException(AuthConstants.Errors.UserContextRequired);

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                throw new ValidationException(AuthConstants.Errors.UserAlreadyExists);

            var invite = new TenantInvite(
                _currentTenant.Id.Value,
                request.Email,
                request.Role,
                _currentUser.Id.Value
            );

            await _tenantInviteRepository.CreateAsync(invite);
            await _tenantInviteRepository.SaveChangesAsync();

            await _emailService.SendInviteEmailAsync(request.Email, invite.Token, request.Role);
        }

        public async Task AcceptInviteAsync(AcceptInviteRequest request)
        {
            var invite = await _tenantInviteRepository.GetByTokenAsync(request.Token);
            if (invite == null || invite.IsConsumed || invite.ExpiresAt < DateTime.UtcNow)
                throw new ValidationException(AuthConstants.Errors.InviteInvalidOrExpired);

            if (!invite.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase))
                throw new ValidationException(AuthConstants.Errors.InvalidEmail);

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                TenantId = invite.TenantId,
                EmailConfirmed = true
            };
            user.MarkEmailAsVerified();

            user = await _userDomainService.CreateUserAsync(user, request.Password);

            var role = await _roleManager.FindByNameAsync(invite.Role);
            if (role != null)
            {
                await _userTenantRoleRepository.CreateAsync(new UserTenantRole(user.Id, role.Id, invite.TenantId));
            }

            invite.IsConsumed = true;
            _tenantInviteRepository.Update(invite);
            await _tenantInviteRepository.SaveChangesAsync();
        }

        public async Task BlockUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            ValidateUser(user);

            await _userManager.SetLockoutEnabledAsync(user!, true);
            await _userManager.SetLockoutEndDateAsync(user!, DateTimeOffset.MaxValue);
        }

        public async Task UnblockUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            ValidateUser(user);

            await _userManager.SetLockoutEndDateAsync(user!, null);
        }
    }
}
