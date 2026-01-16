using Microsoft.Extensions.Configuration;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Enums;
using Ona.Commit.Domain.Interfaces.Repositories;
using Ona.Core.Tenant;
using System.Globalization;

namespace Ona.Commit.Infrastructure.Integrations
{
    public class TemplateMessageBuilder : ITemplateMessageBuilder
    {
        private readonly ITenantProvider _tenantProvider;
        private readonly IMessageTemplateRepository _templateRepository;
        private readonly IConfiguration _configuration;

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
                content = "Olá {{NomePaciente}}, este é um lembrete do seu agendamento na {{NomeClinica}} para o dia {{DataHora}}.\r\nConfirme aqui: {{LinkConfirmacao}}";
            }

            content = content
                .Replace("{{NomePaciente}}", patientName)
                .Replace("{{NomeClinica}}", clinicName)
                .Replace("{{DataHora}}", appointmentDate)
                .Replace("{{LinkConfirmacao}}", confirmationLink);

            return content;
        }
    }
}
