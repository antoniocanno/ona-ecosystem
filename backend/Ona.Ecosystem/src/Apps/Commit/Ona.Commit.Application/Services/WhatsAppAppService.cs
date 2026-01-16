using Microsoft.Extensions.Logging;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Interfaces.Gateways;

namespace Ona.Commit.Application.Services;

public class WhatsAppAppService : IWhatsAppAppService
{
    private readonly IWhatsAppGateway _whatsAppGateway;
    private readonly ILogger<WhatsAppAppService> _logger;

    public WhatsAppAppService(
        IWhatsAppGateway whatsAppGateway,
        ILogger<WhatsAppAppService> logger)
    {
        _whatsAppGateway = whatsAppGateway;
        _logger = logger;
    }

    public async Task<WhatsAppInstanceResponse> ConnectAsync(Guid tenantId)
    {
        var instanceName = $"tenant_{tenantId:N}";

        WhatsAppConnectionStatus? status = null;
        bool instanceExists = false;

        try
        {
            status = await _whatsAppGateway.GetConnectionStatusAsync(instanceName);

            if (status.State != "not_found")
                instanceExists = true;
            else
                instanceExists = false;
        }
        catch (Exception)
        {
            _logger.LogError("Erro ao buscar status da instância {InstanceName}", instanceName);
            throw;
        }

        if (instanceExists && status != null && status.IsConnected)
        {
            return new WhatsAppInstanceResponse
            {
                InstanceName = instanceName,
                Status = "connected",
                QrCodeBase64 = null
            };
        }

        if (!instanceExists)
        {
            try
            {
                await _whatsAppGateway.CreateInstanceAsync(tenantId, instanceName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar instância {InstanceName}", instanceName);
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
