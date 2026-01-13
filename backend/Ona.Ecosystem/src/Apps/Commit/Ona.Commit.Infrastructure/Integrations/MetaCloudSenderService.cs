using Microsoft.Extensions.Logging;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace Ona.Commit.Infrastructure.Integrations
{
    public class MetaCloudSenderService : IMetaCloudSenderService
    {
        private readonly IWhatsAppClientFactory _clientFactory;
        private readonly ILogger<MetaCloudSenderService> _logger;

        public MetaCloudSenderService(
            IWhatsAppClientFactory clientFactory,
            ILogger<MetaCloudSenderService> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public async Task<string> SendMessageAsync(Guid tenantId, string payload)
        {
            var client = await _clientFactory.CreateClientAsync(tenantId);

            HttpResponseMessage response;
            try
            {
                var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                response = await client.PostAsync("messages", content);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de rede ao conectar com WhatsApp Graph API.");
                throw new WhatsAppTransientException("Falha na conexão com a Meta.", ex);
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                HandleErrorResponse(response.StatusCode, responseBody);
            }

            try
            {
                var json = JsonDocument.Parse(responseBody);
                if (json.RootElement.TryGetProperty("messages", out var messagesElement) &&
                    messagesElement.GetArrayLength() > 0)
                {
                    var id = messagesElement[0].GetProperty("id").GetString();
                    if (!string.IsNullOrEmpty(id))
                    {
                        return id;
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Erro ao parsear resposta de sucesso da Meta. Body: {Body}", responseBody);
                throw new WhatsAppTransientException("Resposta inválida da API da Meta.", ex);
            }

            throw new WhatsAppTransientException($"Resposta inesperada da API da Meta: {responseBody}");
        }

        private void HandleErrorResponse(HttpStatusCode statusCode, string responseBody)
        {
            _logger.LogError("Erro da API da Meta. Status: {Status}. Body: {Body}", statusCode, responseBody);

            string errorCode = "";
            string errorType = "";
            string message = "";

            try
            {
                var json = JsonDocument.Parse(responseBody);
                if (json.RootElement.TryGetProperty("error", out var errorElement))
                {
                    message = errorElement.GetProperty("message").GetString() ?? "";
                    if (errorElement.TryGetProperty("code", out var codeEl)) errorCode = codeEl.ToString();
                    if (errorElement.TryGetProperty("type", out var typeEl)) errorType = typeEl.GetString() ?? "";
                }
            }
            catch
            {
                throw new WhatsAppTransientException($"Erro HTTP {statusCode} da Meta: {responseBody}");
            }

            // Tratamento de erros comuns
            // Referência: https://developers.facebook.com/docs/whatsapp/cloud-api/support/error-codes

            if (errorCode == "190")
            {
                throw new WhatsAppPermanentException($"Token de acesso inválido ou expirado: {message}");
            }

            if (errorCode == "130429" || errorCode == "131030" || errorCode == "80007")
            {
                throw new WhatsAppTransientException($"Limite de taxa atingido (Rate Limit): {message}");
            }
            if (statusCode == HttpStatusCode.InternalServerError || statusCode == HttpStatusCode.ServiceUnavailable)
            {
                throw new WhatsAppTransientException($"Erro interno da Meta: {message}");
            }

            if (errorCode == "132001")
            {
                throw new WhatsAppPermanentException($"Template inválido ou não aprovado: {message}");
            }

            if (errorCode == "131026")
            {
                throw new WhatsAppPermanentException($"Mensagem não enviável: {message}");
            }

            throw new WhatsAppTransientException($"Erro na API da Meta ({errorCode}): {message}");
        }
    }
}
