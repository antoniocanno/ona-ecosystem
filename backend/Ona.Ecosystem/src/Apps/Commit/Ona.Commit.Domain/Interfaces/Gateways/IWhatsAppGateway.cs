using Ona.Commit.Domain.Entities;

namespace Ona.Commit.Domain.Interfaces.Gateways
{
    /// <summary>
    /// Gateway para integração com provedores de WhatsApp (Evolution API, Meta Cloud API, etc.)
    /// </summary>
    public interface IWhatsAppGateway
    {
        /// <summary>
        /// Cria uma nova instância do WhatsApp para um tenant
        /// </summary>
        /// <param name="tenantId">ID do Tenant</param>
        /// <param name="instanceName">Nome da instância (geralmente o nome do tenant ou ID único)</param>
        /// <param name="proxy">Proxy opcional para a instância</param>
        /// <returns>Retorna informações sobre a instância criada, incluindo o QR Code se aplicável</returns>
        Task<WhatsAppInstanceResponse> CreateInstanceAsync(Guid tenantId, string instanceName, ProxyServer? proxy = null);

        /// <summary>
        /// Obtém o QR Code para conectar uma instância do WhatsApp
        /// </summary>
        /// <param name="instanceName">Nome da instância</param>
        /// <returns>Retorna o QR Code em formato Base64 e informações sobre o estado da conexão</returns>
        Task<WhatsAppQrCodeResponse> GetQrCodeAsync(string instanceName);

        /// <summary>
        /// Reinicia uma instância do WhatsApp (Obtém um novo QR Code)
        /// </summary>
        /// <param name="instanceName">Nome da instância</param>
        /// <returns>Retorna o QR Code em formato Base64 e informações sobre o estado da conexão</returns>
        Task<WhatsAppQrCodeResponse> RestartInstanceAsync(string instanceName);

        /// <summary>
        /// Verifica o status da conexão de uma instância
        /// </summary>
        /// <param name="instanceName">Nome da instância</param>
        /// <returns>Retorna informações sobre o estado da conexão</returns>
        Task<WhatsAppConnectionStatus> GetConnectionStatusAsync(string instanceName);

        /// <summary>
        /// Desconecta e remove uma instância do WhatsApp
        /// </summary>
        /// <param name="instanceName">Nome da instância</param>
        Task DeleteInstanceAsync(string instanceName);

        /// <summary>
        /// Envia uma mensagem de texto via WhatsApp
        /// </summary>
        /// <param name="instanceName">Nome da instância</param>
        /// <param name="phoneNumber">Número de telefone do destinatário (formato internacional)</param>
        /// <param name="message">Mensagem a ser enviada</param>
        /// <returns>ID da mensagem enviada</returns>
        Task<string> SendTextMessageAsync(string instanceName, string phoneNumber, string message);
        /// <summary>
        /// Envia uma mensagem com botões interativos via WhatsApp
        /// </summary>
        /// <param name="instanceName">Nome da instância</param>
        /// <param name="phoneNumber">Número de telefone do destinatário</param>
        /// <param name="title">Título da mensagem</param>
        /// <param name="description">Descrição/Corpo da mensagem</param>
        /// <param name="footer">Rodapé da mensagem</param>
        /// <param name="buttons">Lista de botões</param>
        /// <returns>ID da mensagem enviada</returns>
        Task<string> SendButtonsMessageAsync(string instanceName, string phoneNumber, string title, string description, string footer, List<WhatsAppButton> buttons);

        /// <summary>
        /// Atualiza as configurações de proxy de uma instância existente.
        /// </summary>
        /// <param name="instanceName">Nome da instância</param>
        /// <param name="proxy">Configurações de proxy</param>
        Task SetProxyAsync(string instanceName, ProxyServer proxy);

        /// <summary>
        /// Configura a integração com RabbitMQ para uma instância existente.
        /// </summary>
        /// <param name="instanceName">Nome da instância</param>
        Task SetRabbitMqConfigAsync(string instanceName);
    }
}

namespace Ona.Commit.Domain.Interfaces.Gateways
{
    public class WhatsAppButton
    {
        public string Type { get; set; } = "reply";
        public string DisplayText { get; set; } = string.Empty;
        public string? Id { get; set; }
        public string? CopyCode { get; set; }
        public string? Url { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Currency { get; set; }
        public string? Name { get; set; }
        public string? KeyType { get; set; }
        public string? Key { get; set; }
    }

    public class WhatsAppInstanceResponse
    {
        public string InstanceName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? QrCodeBase64 { get; set; }
    }

    public class WhatsAppQrCodeResponse
    {
        public string QrCodeBase64 { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? ExpiresIn { get; set; }
    }

    public class WhatsAppConnectionStatus
    {
        public string InstanceName { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public bool IsConnected { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
