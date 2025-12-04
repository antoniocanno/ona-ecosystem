namespace Ona.Auth.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendEmailVerificationAsync(string email, string verificationToken, string userName);
        Task SendPasswordResetEmailAsync(string email, string passwordResetToken, string userName);
        Task SendLockoutNotificationAsync(string email, string lockoutToken, string userName);
    }
}
