using Ona.Auth.Application.Models;

namespace Ona.Auth.Application.Interfaces.Services
{
    public interface IEmailTemplateService
    {
        Task<string> RenderEmailVerificationAsync(EmailVerificationModel model);
        Task<string> RenderEmailPasswordResetAsync(PasswordResetModel model);
        Task<string> RenderEmailLockoutNotificationAsync(UnlockUserModel model);
    }
}
