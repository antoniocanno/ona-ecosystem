using Microsoft.Extensions.Logging;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Interfaces.Gateways;
using Ona.Commit.Domain.Interfaces.Repositories;

namespace Ona.Commit.Application.Services;

public class WhatsAppAppService : IWhatsAppAppService
{
    private readonly IWhatsAppGateway _whatsAppGateway;
    private readonly IProxyResourceManager _proxyResourceManager;
    private readonly ITenantWhatsAppConfigRepository _configRepository;
    private readonly ILogger<WhatsAppAppService> _logger;

    public WhatsAppAppService(
        IWhatsAppGateway whatsAppGateway,
        IProxyResourceManager proxyResourceManager,
        ITenantWhatsAppConfigRepository configRepository,
        ILogger<WhatsAppAppService> logger)
    {
        _whatsAppGateway = whatsAppGateway;
        _proxyResourceManager = proxyResourceManager;
        _configRepository = configRepository;
        _logger = logger;
    }

    public async Task<WhatsAppInstanceResponse> ConnectAsync(Guid tenantId)
    {
        var instanceName = $"tenant_{tenantId:N}";

        try
        {
            var status = await _whatsAppGateway.GetConnectionStatusAsync(instanceName);

            if (status.State != "not_found" && status.IsConnected)
            {
                return new WhatsAppInstanceResponse
                {
                    InstanceName = instanceName,
                    Status = "connected",
                    QrCodeBase64 = null
                };
            }

            if (status.State == "not_found")
            {
                ProxyServer? proxy = null;
                try
                {
                    proxy = await _proxyResourceManager.AllocateProxyAsync(tenantId);
                    await _whatsAppGateway.CreateInstanceAsync(tenantId, instanceName, proxy);

                    var config = await _configRepository.GetOrCreateByTenantIdAsync(tenantId);
                    config.UpdateEvolutionCredentials(instanceName, null);
                    await _configRepository.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao criar instância Evolution para {InstanceName}. Liberando recursos...", instanceName);

                    if (proxy != null)
                    {
                        await _proxyResourceManager.ReleaseProxyAsync(tenantId);
                    }

                    throw;
                }
            }

            try
            {
                var qrCodeResponse = await _whatsAppGateway.GetQrCodeAsync(instanceName);

                return new WhatsAppInstanceResponse
                {
                    InstanceName = instanceName,
                    Status = qrCodeResponse.Status,
                    QrCodeBase64 = qrCodeResponse.QrCodeBase64
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar QR Code para {InstanceName}", instanceName);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro geral no processamento de conexão para {InstanceName}", instanceName);
            throw;
        }
    }

    public async Task<WhatsAppQrCodeResponse> GetQrCodeAsync(Guid tenantId)
    {
        return await _whatsAppGateway.GetQrCodeAsync($"tenant_{tenantId:N}");
    }

    public async Task<WhatsAppConnectionStatus> GetStatusAsync(Guid tenantId)
    {
        return await _whatsAppGateway.GetConnectionStatusAsync($"tenant_{tenantId:N}");
    }

    public async Task DisconnectAsync(Guid tenantId)
    {
        await _whatsAppGateway.DeleteInstanceAsync($"tenant_{tenantId:N}");
    }

    public async Task<string> SendTestMessageAsync(Guid tenantId, string phoneNumber, string message)
    {
        return await _whatsAppGateway.SendTextMessageAsync(
            $"tenant_{tenantId:N}",
            phoneNumber,
            message
        );
    }

    public async Task<string> SendButtonsMessageAsync(Guid tenantId, string phoneNumber, string title, string description, string footer, List<WhatsAppButton> buttons)
    {
        return await _whatsAppGateway.SendButtonsMessageAsync(
            $"tenant_{tenantId:N}",
            phoneNumber,
            title,
            description,
            footer,
            buttons
        );
    }
}
