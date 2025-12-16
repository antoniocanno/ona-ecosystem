using Microsoft.AspNetCore.Identity;
using Ona.Auth.Domain.Constants;
using Ona.Auth.Domain.Entities;
using Ona.Auth.Domain.Interfaces.Services;
using Ona.Core.Common.Exceptions;

namespace Ona.Auth.Domain.Services
{
    public class UserDomainService : IUserDomainService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserDomainService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApplicationUser> CreateUserAsync(ApplicationUser user, string? password = null)
        {
            // Regras de domínio comuns na criação
            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                user.UserName = user.Email;
            }

            IdentityResult result;
            if (!string.IsNullOrEmpty(password))
            {
                result = await _userManager.CreateAsync(user, password);
            }
            else
            {
                result = await _userManager.CreateAsync(user);
            }

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ValidationException(string.Format(AuthConstants.Errors.UserCreationError, errors));
            }

            return user;
        }

        public async Task<ApplicationUser> CreateGoogleUserAsync(string email, string name, string googleId, bool emailVerified)
        {
            var user = new ApplicationUser
            {
                FullName = name,
                Email = email,
                UserName = email,
                GoogleId = googleId,
                EmailConfirmed = emailVerified
            };

            if (emailVerified)
                user.MarkEmailAsVerified();

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ValidationException(string.Format(AuthConstants.Errors.GoogleUserCreationError, errors));
            }

            return user;
        }

        public Task ValidateUserForLoginAsync(ApplicationUser? user)
        {
            if (user == null)
                throw new ForbiddenException(AuthConstants.Errors.InvalidCredentials);

            return Task.CompletedTask;
        }
    }
}
