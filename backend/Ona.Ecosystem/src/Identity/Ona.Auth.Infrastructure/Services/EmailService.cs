using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Ona.Auth.Application.Interfaces.Services;
using Ona.Auth.Application.Settings;
using Ona.Core.Common.Exceptions;

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

            var sanitizedUserName = WebUtility.HtmlEncode(userName);

            var subject = "Confirme seu email";
            var body = $"<p>Seja bem-vindo(a), {sanitizedUserName}</p>" +
                       $"<p>Falta pouco para você começar a usar a plataforma.</p>" +
                       $"<p>Clique <a href='{verificationUrl}'>aqui</a> para confirmar seu email.</p>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string email, string passwordResetToken, string userName)
        {
            var passwordResetUrl = $"https://localhost:7029/api/auth/reset-password?token={passwordResetToken}";

            var sanitizedUserName = WebUtility.HtmlEncode(userName);

            var subject = "Redefina a sua senha";
            var body = $"<p>Olá {sanitizedUserName},</p>" +
                       $"<p>Você solicitou a redefinição de senha. Clique no link abaixo para redefinir sua senha.</p>" +
                       $"<p>Se você não solicitou a redefinição de senha, ignore este email.</p>" +
                       $"<p>Clique <a href='{passwordResetUrl}'>aqui</a> para redefinir sua senha.</p>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendLockoutNotificationAsync(string email, string lockoutToken, string userName)
        {
            var unlockUserUrl = $"https://localhost:7029/api/auth/unlock-user?token={lockoutToken}&email={email}";

            var sanitizedUserName = WebUtility.HtmlEncode(userName);

            var subject = "Seu email foi bloqueado";
            var body = $"<p>Olá {sanitizedUserName},</p>" +
                       $"<p>Por motivos de segurança, seu email foi bloqueado.</p>" +
                       $"<p>Clique <a href='{unlockUserUrl}'>aqui</a> para desbloquear sua conta.</p>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendInviteEmailAsync(string email, string inviteToken, string role)
        {
            var inviteUrl = $"https://localhost:7029/api/auth/accept-invite?token={inviteToken}&email={email}";

            var subject = "Convite para acessar a plataforma";
            var body = $"<p>Você foi convidado para acessar a plataforma com o perfil de <strong>{role}</strong>.</p>" +
                       $"<p>Clique <a href='{inviteUrl}'>aqui</a> para aceitar o convite e criar sua senha.</p>";

            await SendEmailAsync(email, subject, body);
        }

        /// <summary>
        /// Template email model
        /// </summary>
        /*
        public async Task SendEmailTemplateAsync(string email, string token, string userName)
        {
            var url = $"https://localhost:7029/api/auth/accept-template?token={token}&email={email}";

            var model = new TemplateModel
            {
                UserName = userName,
                Url = url,
                SupportEmail = _emailSettings.SupportEmail,
                CompanyName = _emailSettings.CompanyName
            };

            var subject = "Template Email";
            var template = await _templateService.RenderEmailTemplateAsync(model);

            var sanitizedUserName = WebUtility.HtmlEncode(userName);

            var body = template
                .Replace("{{userName}}", sanitizedUserName)
                .Replace("{{url}}", url);

            await SendEmailAsync(email, subject, body);
        }
        */

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
                throw new IntegrationException("SMTP", $"Falha ao enviar email para {toEmail}", ex, isTransient: true);
            }
        }
    }
}
