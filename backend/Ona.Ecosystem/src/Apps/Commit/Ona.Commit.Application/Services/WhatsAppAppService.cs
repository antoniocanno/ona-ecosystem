using Microsoft.Extensions.Logging;
using Ona.Application.Shared.DTOs.Tenants;
using Ona.Application.Shared.Interfaces.Services;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Interfaces.Gateways;

namespace Ona.Commit.Application.Services;

public class WhatsAppAppService : IWhatsAppAppService
{
    private readonly ITenantService _tenantService;
    private readonly IWhatsAppGateway _whatsAppGateway;
    private readonly ILogger<WhatsAppAppService> _logger;

    public WhatsAppAppService(
        ITenantService tenantService,
        IWhatsAppGateway whatsAppGateway,
        ILogger<WhatsAppAppService> logger)
    {
        _tenantService = tenantService;
        _whatsAppGateway = whatsAppGateway;
        _logger = logger;
    }

    public async Task<WhatsAppInstanceResponse> ConnectAsync(Guid tenantId)
    {
        var instanceName = $"tenant_{tenantId:N}";

        // Tenta obter o status primeiro para verificar se já existe uma instância
        WhatsAppConnectionStatus? existingStatus = null;
        try
        {
            existingStatus = await _whatsAppGateway.GetConnectionStatusAsync(instanceName);

            // Se já está conectado, retorna o status
            if (existingStatus.IsConnected)
            {
                return new WhatsAppInstanceResponse
                {
                    InstanceName = instanceName,
                    Status = "connected",
                    QrCodeBase64 = null
                };
            }
        }
        catch (Exception)
        {
            // Instância não existe ainda, vamos criar
            _logger.LogInformation("Instância não existe, criando nova para tenant {TenantId}", tenantId);
        }

        // Cria ou reconecta a instância
        WhatsAppInstanceResponse instanceResponse;
        try
        {
            instanceResponse = await _whatsAppGateway.CreateInstanceAsync(tenantId, instanceName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar instância para tenant {TenantId}", tenantId);

            var qrCodeResponse = await _whatsAppGateway.GetQrCodeAsync(instanceName);
            return new WhatsAppInstanceResponse
            {
                InstanceName = instanceName,
                Status = qrCodeResponse.Status,
                QrCodeBase64 = qrCodeResponse.QrCodeBase64
            };
        }

        return instanceResponse;
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
}
