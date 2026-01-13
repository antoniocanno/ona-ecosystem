namespace Ona.Commit.Application.Interfaces.Services
{
    public interface IWhatsAppClientFactory
    {
        /// <summary>
        /// Cria e configura um HttpClient para interagir com a API do WhatsApp/Meta.
        /// A configuração (Token) é resolvida com base no TenantId:
        /// - Se o Tenant tiver WABA próprio, usa suas credenciais.
        /// - Caso contrário, usa a conta compartilhada do sistema.
        /// </summary>
        /// <param name="tenantId">ID do Tenant</param>
        /// <returns>HttpClient configurado com a BaseUrl e Authorization Token corretos.</returns>
        Task<HttpClient> CreateClientAsync(Guid tenantId);
    }
}
