using MassTransit;
using Microsoft.Extensions.Logging;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Interfaces.Gateways;
using Ona.Commit.Domain.Interfaces.Repositories;
using Ona.Commit.Infrastructure.Gateways.Evolution.Events;
using System.Text.Json;

namespace Ona.Commit.Infrastructure.Gateways.Evolution.Consumers
{
    public class EvolutionEventConsumer : IConsumer<EvolutionMessageEvent>
    {
        private readonly ILogger<EvolutionEventConsumer> _logger;
        private readonly IProxyResourceManager _proxyResourceManager;
        private readonly ITenantWhatsAppConfigRepository _configRepository;
        private readonly IWhatsAppGateway _whatsAppGateway;

        private const string ConnectionUpdate = "connection.update";
        private const string MessagesUpsert = "messages.upsert";

        public EvolutionEventConsumer(
            ILogger<EvolutionEventConsumer> logger,
            IProxyResourceManager proxyResourceManager,
            ITenantWhatsAppConfigRepository configRepository,
            IWhatsAppGateway whatsAppGateway)
        {
            _logger = logger;
            _proxyResourceManager = proxyResourceManager;
            _configRepository = configRepository;
            _whatsAppGateway = whatsAppGateway;
        }

        public async Task Consume(ConsumeContext<EvolutionMessageEvent> context)
        {
            var evento = context.Message;
            _logger.LogInformation("Evento recebido do RabbitMQ: {Event} para instância {Instance}", evento.Event, evento.Instance);

            if (evento.Event == ConnectionUpdate)
            {
                await HandleConnectionUpdateAsync(evento);
            }
            else if (evento.Event == MessagesUpsert)
            {
                await HandleMessagesUpsertAsync(evento);
            }
        }

        private async Task HandleConnectionUpdateAsync(EvolutionMessageEvent evento)
        {
            try
            {
                if (evento.Data is JsonElement dataElement &&
                    dataElement.TryGetProperty("state", out var stateProp))
                {
                    var state = stateProp.GetString();

                    if (state == "refused" || state == "close")
                    {
                        _logger.LogWarning("Detectada falha de conexão via RabbitMQ para instância {InstanceName}. Estado: {State}", evento.Instance, state);

                        var config = await _configRepository.GetByInstanceNameAsync(evento.Instance);

                        if (config == null && evento.Instance.StartsWith("tenant_"))
                        {
                            var tenantIdStr = evento.Instance.Replace("tenant_", "");
                            if (Guid.TryParse(tenantIdStr, out var tenantId))
                            {
                                config = await _configRepository.GetOrCreateByTenantIdAsync(tenantId);
                                if (string.IsNullOrEmpty(config.InstanceName))
                                {
                                    config.UpdateEvolutionCredentials(evento.Instance, null);
                                    await _configRepository.SaveChangesAsync();
                                }
                            }
                        }

                        if (config?.ProxyServerId != null)
                        {
                            _logger.LogInformation("Tentando rotacionar proxy para tenant {TenantId}...", config.TenantId);

                            var newProxy = await _proxyResourceManager.RotateProxyAsync(config.TenantId);

                            if (newProxy != null)
                            {
                                _logger.LogInformation("Novo proxy alocado: {ProxyHost}. Atualizando instância...", newProxy.Host);
                                await _whatsAppGateway.SetProxyAsync(evento.Instance, newProxy);
                            }
                            else
                            {
                                _logger.LogError("Não foi possível alocar um novo proxy para o tenant {TenantId}.", config.TenantId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento connection.update via RabbitMQ");
                throw;
            }
        }

        private async Task HandleMessagesUpsertAsync(EvolutionMessageEvent evento)
        {
            try
            {
                if (evento.Data is JsonElement dataElement &&
                    dataElement.TryGetProperty("status", out var statusProp))
                {
                    var status = statusProp.GetString();

                    if (status == "read")
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento messages.upsert via RabbitMQ");
                throw;
            }
        }
    }
}
