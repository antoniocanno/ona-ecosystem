namespace Ona.Commit.Application.Interfaces.Services
{
    public interface IMetaCloudSenderService
    {
        /// <summary>
        /// Envia uma mensagem para a Graph API do WhatsApp.
        /// </summary>
        /// <param name="tenantId">ID do Tenant para resolução do cliente HTTP.</param>
        /// <param name="payload">JSON completo da mensagem a ser enviada.</param>
        /// <returns>ID da mensagem gerado pela Meta (ExternalMessageId).</returns>
        Task<string> SendMessageAsync(Guid tenantId, string payload);
    }
}
