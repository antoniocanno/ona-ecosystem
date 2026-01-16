using Microsoft.Extensions.Configuration;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Interfaces;
using Ona.Commit.Domain.Interfaces.Repositories;
using Ona.Core.Tenant;
using System.Net.Http.Headers;

namespace Ona.Commit.Infrastructure.Integrations
{
    public class WhatsAppClientFactory : IWhatsAppClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITenantWhatsAppConfigRepository _configRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICryptographyService _cryptoService;
        private readonly IConfiguration _configuration;

        public WhatsAppClientFactory(
            IHttpClientFactory httpClientFactory,
            ITenantWhatsAppConfigRepository configRepository,
            ITenantProvider tenantProvider,
            ICryptographyService cryptoService,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configRepository = configRepository;
            _tenantProvider = tenantProvider;
            _cryptoService = cryptoService;
            _configuration = configuration;
        }

        public async Task<HttpClient> CreateClientAsync(Guid tenantId)
        {
            var config = await _configRepository.GetByTenantIdAsync(tenantId);
            var client = _httpClientFactory.CreateClient("WhatsAppClient");

            string? token;
            string? phoneNumberId;

            if (config != null && !config.IsUsingSharedAccount && !string.IsNullOrEmpty(config.PhoneNumberId))
            {
                // TODO: Carregar o AccessToken específico do Tenant se houver. 
                var tenant = await _tenantProvider.GetAsync(tenantId);
                token = tenant?.WhatsAppApiKey;
                phoneNumberId = config.PhoneNumberId;
                // Se estiver encriptado, descriptografar:
                // phoneNumberId = _cryptoService.Decrypt(config.PhoneNumberId);
            }
            else
            {
                // Shared Account
                token = _configuration["WhatsApp:MasterToken"];
                phoneNumberId = _configuration["WhatsApp:MasterPhoneNumberId"];
            }

            if (string.IsNullOrEmpty(token))
                throw new InvalidOperationException($"Token do WhatsApp não configurado para o Tenant {tenantId}");

            if (string.IsNullOrEmpty(phoneNumberId))
                throw new InvalidOperationException($"PhoneNumberId do WhatsApp não configurado para o Tenant {tenantId}");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // A URL base da Meta geralmente inclui o ID do número de telefone
            // Ex: https://graph.facebook.com/v21.0/{phoneNumberId}/
            var apiVersion = _configuration["WhatsApp:ApiVersion"] ?? "v21.0";
            client.BaseAddress = new Uri($"https://graph.facebook.com/{apiVersion}/{phoneNumberId}/");

            return client;
        }
    }
}
