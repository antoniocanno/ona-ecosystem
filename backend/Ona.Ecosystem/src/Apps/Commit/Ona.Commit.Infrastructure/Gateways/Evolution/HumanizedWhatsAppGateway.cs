using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Interfaces.Gateways;
using Ona.Commit.Domain.Interfaces.Repositories;

namespace Ona.Commit.Infrastructure.Gateways.Evolution
{
    /// <summary>
    /// Gateway decorador que adiciona "humanização" (delay + fila) no envio de mensagens
    /// para evitar bloqueios do WhatsApp.
    /// </summary>
    public class HumanizedWhatsAppGateway : IWhatsAppGateway
    {
        private readonly EvolutionWhatsAppGateway _client;
        private readonly EvolutionMessageDispatcher _dispatcher;
        private readonly IWhatsAppNumberVerificationRepository _verificationRepository;
        private readonly ITenantWhatsAppConfigRepository _configRepository;
        private readonly IMessageInteractionLogRepository _logRepository;

        public HumanizedWhatsAppGateway(
            EvolutionWhatsAppGateway client,
            EvolutionMessageDispatcher dispatcher,
            IWhatsAppNumberVerificationRepository verificationRepository,
            ITenantWhatsAppConfigRepository configRepository,
            IMessageInteractionLogRepository logRepository)
        {
            _client = client;
            _dispatcher = dispatcher;
            _verificationRepository = verificationRepository;
            _configRepository = configRepository;
            _logRepository = logRepository;
        }

        public Task<WhatsAppInstanceResponse> CreateInstanceAsync(Guid tenantId, string instanceName)
            => _client.CreateInstanceAsync(tenantId, instanceName);

        public Task<WhatsAppQrCodeResponse> GetQrCodeAsync(string instanceName)
            => _client.GetQrCodeAsync(instanceName);

        public Task<WhatsAppQrCodeResponse> RestartInstanceAsync(string instanceName)
            => _client.RestartInstanceAsync(instanceName);

        public Task<WhatsAppConnectionStatus> GetConnectionStatusAsync(string instanceName)
            => _client.GetConnectionStatusAsync(instanceName);

        public Task DeleteInstanceAsync(string instanceName)
            => _client.DeleteInstanceAsync(instanceName);

        public Task<string> SendTextMessageAsync(string instanceName, string phoneNumber, string message)
        {
            return _dispatcher.EnqueueAsync(instanceName, async (client) =>
            {
                await ValidateWarmUpLimitAsync(instanceName);
                await ValidateNumberAsync(instanceName, phoneNumber, client);

                var typingDuration = new Random().Next(2000, 4500);

                await client.SendPresenceAsync(instanceName, phoneNumber, "composing", typingDuration);

                await Task.Delay(typingDuration);

                return await client.SendTextMessageAsync(instanceName, phoneNumber, message);
            });
        }

        public Task<string> SendButtonsMessageAsync(string instanceName, string phoneNumber, string title, string description, string footer, List<WhatsAppButton> buttons)
        {
            return _dispatcher.EnqueueAsync(instanceName, async (client) =>
            {
                await ValidateWarmUpLimitAsync(instanceName);
                await ValidateNumberAsync(instanceName, phoneNumber, client);

                var typingDuration = new Random().Next(2000, 4500);

                await client.SendPresenceAsync(instanceName, phoneNumber, "composing", typingDuration);
                await Task.Delay(typingDuration);

                return await client.SendButtonsMessageAsync(instanceName, phoneNumber, title, description, footer, buttons);
            });
        }

        private async Task ValidateNumberAsync(string instanceName, string phoneNumber, EvolutionWhatsAppGateway client)
        {
            var cleanPhone = phoneNumber.Replace("+", "").Replace("-", "").Replace(" ", "");
            var cached = await _verificationRepository.GetByPhoneNumberAsync(cleanPhone);

            if (cached != null)
            {
                if (!cached.IsValid)
                {
                    throw new InvalidOperationException($"O número {phoneNumber} é inválido no WhatsApp e foi bloqueado preventivamente.");
                }
            }
            else
            {
                var isValid = await client.CheckNumberAsync(instanceName, phoneNumber);

                var verification = new WhatsAppNumberVerification(cleanPhone, isValid);

                await _verificationRepository.CreateAsync(verification);
                await _verificationRepository.SaveChangesAsync();
            }
        }

        private async Task ValidateWarmUpLimitAsync(string instanceName)
        {
            if (!instanceName.StartsWith("tenant_")) return;

            var tenantIdString = instanceName.Replace("tenant_", "");
            if (!Guid.TryParse(tenantIdString, out var tenantId))
            {
                return;
            }

            var config = await _configRepository.GetByTenantIdAsync(tenantId);
            if (config == null) return;

            var daysActive = (DateTimeOffset.UtcNow - config.CreatedAt).TotalDays;
            int limit = daysActive switch
            {
                <= 3 => 50,
                <= 7 => 100,
                _ => 1000,
            };

            var sentToday = await _logRepository.CountMessagesSentTodayAsync(tenantId);

            if (sentToday >= limit)
            {
                throw new InvalidOperationException($"Limite diário de aquecimento atingido para esta instância. Idade: {daysActive:F1} dias. Limite: {limit} mensagens. Enviadas hoje: {sentToday}.");
            }

            var brazilTime = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-3));
            if (brazilTime.Hour is < 8 or >= 20)
            {
                throw new InvalidOperationException($"Envio bloqueado fora do horário comercial (08:00 - 20:00). Hora atual: {brazilTime:HH:mm}.");
            }
        }
    }
}
