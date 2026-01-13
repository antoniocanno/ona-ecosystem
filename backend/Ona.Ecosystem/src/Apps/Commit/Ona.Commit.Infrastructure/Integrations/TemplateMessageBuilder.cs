using Newtonsoft.Json;
using Ona.Application.Shared.Interfaces.Services;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Interfaces.Repositories;
using System.Globalization;

namespace Ona.Commit.Infrastructure.Integrations
{
    public class TemplateMessageBuilder : ITemplateMessageBuilder
    {
        private readonly IWhatsAppTemplateRegistryRepository _registryRepository;
        private readonly ITenantWhatsAppConfigRepository _configRepository;
        private readonly ITenantService _tenantService;

        public TemplateMessageBuilder(
            IWhatsAppTemplateRegistryRepository registryRepository,
            ITenantWhatsAppConfigRepository configRepository,
            ITenantService tenantService)
        {
            _registryRepository = registryRepository;
            _configRepository = configRepository;
            _tenantService = tenantService;
        }

        public async Task<string> BuildReminderPayloadAsync(Appointment appointment)
        {
            var tenantId = appointment.TenantId;
            var tenant = await _tenantService.GetByIdAsync(tenantId);
            var clinicName = tenant?.Name ?? "Nossa Clínica";

            var config = await _configRepository.GetByTenantIdAsync(tenantId);
            var isShared = config == null || config.IsUsingSharedAccount;

            var registry = await _registryRepository.GetByLogicalNameAsync(tenantId, "AppointmentReminder");
            var templateName = registry?.MetaTemplateName ?? "appointment_reminder_standard";
            var languageCode = registry?.LanguageCode ?? "pt_BR";

            var patientName = appointment.Customer?.Name ?? "Cliente";

            // Regra: Se compartilhado, Nome da Clínica no início para contexto
            var firstVar = isShared ? $"{clinicName}: {patientName}" : patientName;

            var appointmentDate = appointment.StartDate.ToString("dd/MM/yyyy HH:mm", new CultureInfo("pt-BR"));
            var confirmationLink = $"https://ona.com/confirm/{appointment.Id}";

            // Montagem do payload conforme estrutura da Meta Cloud API
            var payload = new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to = appointment.Customer?.PhoneNumber ?? "",
                type = "template",
                template = new
                {
                    name = templateName,
                    language = new { code = languageCode },
                    components = new[]
                    {
                        new
                        {
                            type = "body",
                            parameters = new[]
                            {
                                new { type = "text", text = firstVar },        // {{1}}
                                new { type = "text", text = clinicName },       // {{2}}
                                new { type = "text", text = appointmentDate },  // {{3}}
                                new { type = "text", text = confirmationLink }  // {{4}}
                            }
                        }
                    }
                }
            };

            return JsonConvert.SerializeObject(payload);
        }
    }
}
