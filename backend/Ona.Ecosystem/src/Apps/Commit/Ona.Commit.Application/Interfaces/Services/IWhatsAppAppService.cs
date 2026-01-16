using Ona.Commit.Domain.Interfaces.Gateways;

namespace Ona.Commit.Application.Interfaces.Services;

/// <summary>
/// Serviço de aplicação para gerenciamento do WhatsApp
/// </summary>
public interface IWhatsAppAppService
{
    /// <summary>
    /// Conecta ou reconecta uma instância do WhatsApp e retorna informações (incluindo QR Code se necessário)
    /// </summary>
    Task<WhatsAppInstanceResponse> ConnectAsync(Guid tenantId);

    /// <summary>
    /// Obtém o QR Code para conectar uma instância do WhatsApp
    /// </summary>
    Task<WhatsAppQrCodeResponse> GetQrCodeAsync(Guid tenantId);

    /// <summary>
    /// Verifica e atualiza o status da conexão
    /// </summary>
    Task<WhatsAppConnectionStatus> GetStatusAsync(Guid tenantId);

    /// <summary>
    /// Desconecta e remove uma instância do WhatsApp
    /// </summary>
    Task DisconnectAsync(Guid tenantId);

    /// <summary>
    /// Envia uma mensagem de teste
    /// </summary>
    Task<string> SendTestMessageAsync(Guid tenantId, string phoneNumber, string message);

    /// <summary>
    /// Envia uma mensagem com botões interativos
    /// </summary>
    Task<string> SendButtonsMessageAsync(Guid tenantId, string phoneNumber, string title, string description, string footer, List<WhatsAppButton> buttons);
}
