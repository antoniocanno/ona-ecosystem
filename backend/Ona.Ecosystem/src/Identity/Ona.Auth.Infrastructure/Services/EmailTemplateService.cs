using Microsoft.Extensions.Options;
using Ona.Auth.Application.Interfaces.Common;
using Ona.Auth.Application.Interfaces.Services;
using Ona.Auth.Application.Models;
using Ona.Auth.Application.Settings;

namespace Ona.Auth.Infrastructure.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IEmailTemplateEngine _templateEngine;
        private readonly EmailSettings _emailSettings;

        public EmailTemplateService(
            IEmailTemplateEngine templateEngine,
            IOptions<EmailSettings> settings)
        {
            _templateEngine = templateEngine;
            _emailSettings = settings.Value;
        }

        public async Task<string> RenderEmailVerificationAsync(EmailVerificationModel model)
        {
            model.CompanyName ??= _emailSettings.CompanyName;
            model.SupportEmail ??= _emailSettings.SupportEmail;

            return await _templateEngine.RenderTemplateAsync("EmailVerification", model);
        }

        public async Task<string> RenderEmailPasswordResetAsync(PasswordResetModel model)
        {
            model.CompanyName ??= _emailSettings.CompanyName;
            model.SupportEmail ??= _emailSettings.SupportEmail;

            return await _templateEngine.RenderTemplateAsync("PasswordReset", model);
        }

        public async Task<string> RenderEmailLockoutNotificationAsync(UnlockUserModel model)
        {
            model.CompanyName ??= _emailSettings.CompanyName;
            model.SupportEmail ??= _emailSettings.SupportEmail;

            return await _templateEngine.RenderTemplateAsync("UnlockUser", model);
        }
    }
}
