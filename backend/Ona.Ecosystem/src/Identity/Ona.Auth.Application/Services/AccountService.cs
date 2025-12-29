using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Ona.Auth.Application.Interfaces.Services;
using Ona.Auth.Application.Settings;
using Ona.Auth.Domain.Constants;
using Ona.Auth.Domain.Entities;
using Ona.Core.Common.Exceptions;
using Ona.Core.Interfaces;

namespace Ona.Auth.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ICacheService _cache;
        private readonly SecuritySettings _securitySettings;
        private readonly ICurrentUser _currentUser;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ICacheService cache,
            IOptions<SecuritySettings> securitySettings,
            ICurrentUser currentUser)
        {
            _userManager = userManager;
            _emailService = emailService;
            _cache = cache;
            _securitySettings = securitySettings.Value;
            _currentUser = currentUser;
        }

        public async Task RequestPasswordResetAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return;

            string counterKey = $"reset_password_attempts:{email}";

            long currentAttempts = await _cache.IncrementAsync(counterKey, TimeSpan.FromHours(1));

            if (currentAttempts >= _securitySettings.AttemptSettings!.MaxAttemptsPerHour)
                throw new ValidationException(AuthConstants.Errors.TooManyAttempts);

            string token = await _userManager.GeneratePasswordResetTokenAsync(user);

            string compositeToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{user.Email}:{token}"));

            await _emailService.SendPasswordResetEmailAsync(user.Email!, compositeToken, user.FullName);
        }

        public async Task ResetPasswordAsync(string token, string newPassword)
        {
            string decodedToken;
            string email;
            string actualToken;

            try
            {
                var bytes = Convert.FromBase64String(token);
                decodedToken = System.Text.Encoding.UTF8.GetString(bytes);
                var parts = decodedToken.Split(':', 2);
                if (parts.Length != 2) throw new Exception();
                email = parts[0];
                actualToken = parts[1];
            }
            catch
            {
                throw new ValidationException(AuthConstants.Errors.InvalidToken);
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) throw new ValidationException(AuthConstants.Errors.UserNotFound);

            var result = await _userManager.ResetPasswordAsync(user, actualToken, newPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ValidationException(string.Format(AuthConstants.Errors.PasswordResetFailed, errors));
            }
        }

        public async Task ChangePasswordAsync(string currentPassword, string newPassword)
        {
            if (!_currentUser.Id.HasValue)
                throw new ValidationException(AuthConstants.Errors.UserContextRequired);

            var userId = _currentUser.Id.Value.ToString();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new ValidationException(AuthConstants.Errors.UserNotFound);

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ValidationException(string.Format(AuthConstants.Errors.PasswordChangeFailed, errors));
            }
        }
    }
}
