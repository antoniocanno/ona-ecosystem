using Microsoft.Extensions.Options;
using Ona.Auth.Application.Interfaces.Services;
using Ona.Auth.Application.Models;
using Ona.Auth.Application.Settings;
using System.Net;
using System.Net.Mail;

namespace Ona.Auth.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IEmailTemplateService _templateService;
        private readonly EmailSettings _emailSettings;
        private readonly SecuritySettings _securitySettings;

        public EmailService(
            IEmailTemplateService emailTemplateService,
            IOptions<EmailSettings> emailSettings,
            IOptions<SecuritySettings> securitySettings)
        {
            _templateService = emailTemplateService;
            _emailSettings = emailSettings.Value;
            _securitySettings = securitySettings.Value;
        }

        public async Task SendEmailVerificationAsync(string email, string verificationToken, string userName)
        {
            var verificationUrl = $"https://localhost:7029/api/auth/verify-email?token={verificationToken}";

            var model = new EmailVerificationModel
            {
                UserName = userName,
                VerificationUrl = verificationUrl,
                ExpirationHours = _securitySettings.EmailVerificationTokenExpiryHours,
                SupportEmail = _emailSettings.SupportEmail,
                CompanyName = _emailSettings.CompanyName
            };

            var template = await _templateService.RenderEmailVerificationAsync(model);
            var subject = "Confirme seu email";

            var sanitizedUserName = WebUtility.HtmlEncode(userName);

            var body = template
                .Replace("{{userName}}", sanitizedUserName)
                .Replace("{{verificationUrl}}", verificationUrl);

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string email, string passwordResetToken, string userName)
        {
            var passwordResetUrl = $"https://localhost:7029/api/auth/reset-password?token={passwordResetToken}";

            var model = new PasswordResetModel
            {
                UserName = userName,
                PasswordResetUrl = passwordResetUrl,
                ExpirationHours = _securitySettings.EmailVerificationTokenExpiryHours,
                SupportEmail = _emailSettings.SupportEmail,
                CompanyName = _emailSettings.CompanyName
            };

            var template = await _templateService.RenderEmailPasswordResetAsync(model);
            var subject = "Redefina a sua senha";

            var sanitizedUserName = WebUtility.HtmlEncode(userName);

            var body = template
                .Replace("{{userName}}", sanitizedUserName)
                .Replace("{{passwordResetUrl}}", passwordResetUrl);

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendLockoutNotificationAsync(string email, string lockoutToken, string userName)
        {
            var unlockUserUrl = $"https://localhost:7029/api/auth/unlock-user?token={lockoutToken}";

            var model = new UnlockUserModel
            {
                UserName = userName,
                UnlockUserUrl = unlockUserUrl,
                ExpirationHours = _securitySettings.EmailVerificationTokenExpiryHours,
                SupportEmail = _emailSettings.SupportEmail,
                CompanyName = _emailSettings.CompanyName
            };

            var template = await _templateService.RenderEmailLockoutNotificationAsync(model);
            var subject = "Redefina a sua senha";

            var sanitizedUserName = WebUtility.HtmlEncode(userName);

            var body = template
                .Replace("{{userName}}", sanitizedUserName)
                .Replace("{{unlockUserUrl}}", unlockUserUrl);

            await SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
                {
                    EnableSsl = _emailSettings.UseSsl,
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                throw new Exception($"Falha ao enviar email: {ex.Message}");
            }
        }
    }
}
