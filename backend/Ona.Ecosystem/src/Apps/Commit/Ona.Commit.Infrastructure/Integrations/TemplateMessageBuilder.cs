using Microsoft.Extensions.Configuration;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Enums;
using Ona.Commit.Domain.Interfaces.Repositories;
using Ona.Core.Tenant;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Ona.Commit.Infrastructure.Integrations
{
    public class TemplateMessageBuilder : ITemplateMessageBuilder
    {
        private readonly ITenantProvider _tenantProvider;
        private readonly IMessageTemplateRepository _templateRepository;
        private readonly IConfiguration _configuration;

        private const string DefaultReminderTemplate = "{Olá|Oi|Ei} {{NomePaciente}}, {tudo bem?|como vai?|} {Este é um lembrete|Passando para lembrar|Gostaria de confirmar} {do seu agendamento|da sua consulta|do seu horário} {na|aqui na} {{NomeClinica}} {para o dia|marcado para} {{DataHora}}.\r\n\r\n{Confirme aqui:|Para confirmar, clique:|Confirme sua presença no link:|Acesse para confirmar:} {{LinkConfirmacao}}";

        public TemplateMessageBuilder(
            ITenantProvider tenantProvider,
            IMessageTemplateRepository templateRepository,
            IConfiguration configuration)
        {
            _tenantProvider = tenantProvider;
            _templateRepository = templateRepository;
            _configuration = configuration;
        }

        public async Task<string> BuildTextReminderAsync(Appointment appointment)
        {
            var tenantId = appointment.TenantId;
            var tenant = await _tenantProvider.GetAsync(tenantId);
            var clinicName = tenant?.Name ?? "Nossa Clínica";
            var patientName = appointment.Customer?.Name ?? "Cliente";
            var appointmentDate = appointment.StartDate.ToString("dd/MM/yyyy 'às' HH:mm", new CultureInfo("pt-BR"));
            var baseUrl = _configuration["App:PublicUrl"]?.TrimEnd('/');
            var confirmationLink = $"{baseUrl}/api/confirmation/{appointment.Id}";

            var template = await _templateRepository.GetByTypeAsync(tenantId, NotificationType.Reminder);

            string content;
            if (template != null)
            {
                content = template.Content;
            }
            else
            {
                content = DefaultReminderTemplate;
            }

            content = content
                .Replace("{{NomePaciente}}", patientName)
                .Replace("{{NomeClinica}}", clinicName)
                .Replace("{{DataHora}}", appointmentDate)
                .Replace("{{LinkConfirmacao}}", confirmationLink);

            return ApplySpintax(content);
        }

        private string ApplySpintax(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var random = new Random();
            var regex = new Regex(@"\{([^{}]+)\}");

            while (regex.IsMatch(text))
            {
                text = regex.Replace(text, match =>
                {
                    var options = match.Groups[1].Value.Split('|');
                    return options[random.Next(options.Length)];
                });
            }

            return text;
        }
    }
}
