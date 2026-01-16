using Microsoft.Extensions.Logging;
using Ona.Commit.Domain.Interfaces.Gateways;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Ona.Commit.Infrastructure.Gateways.Evolution
{
    /// <summary>
    /// Implementação do gateway de WhatsApp usando a Evolution API
    /// </summary>
    public class EvolutionWhatsAppGateway : IWhatsAppGateway
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EvolutionWhatsAppGateway> _logger;

        public EvolutionWhatsAppGateway(
            HttpClient httpClient,
            ILogger<EvolutionWhatsAppGateway> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<WhatsAppInstanceResponse> CreateInstanceAsync(Guid tenantId, string instanceName)
        {
            _logger.LogInformation("Criando instância Evolution API: {InstanceName} para tenant {TenantId}", instanceName, tenantId);

            var request = new
            {
                instanceName,
                qrcode = true,
                integration = "WHATSAPP-BAILEYS"
            };

            var response = await _httpClient.PostAsJsonAsync("/instance/create", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro ao criar instância Evolution: {StatusCode} - {Response}", response.StatusCode, error);
                throw new HttpRequestException($"Falha ao criar instância: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<EvolutionCreateInstanceResponse>();

            return new WhatsAppInstanceResponse
            {
                InstanceName = instanceName,
                Status = result?.Instance?.State ?? "created",
                QrCodeBase64 = result?.Qrcode?.Base64
            };
        }

        public async Task<WhatsAppQrCodeResponse> GetQrCodeAsync(string instanceName)
        {
            _logger.LogInformation("Obtendo QR Code para instância: {InstanceName}", instanceName);

            var response = await _httpClient.GetAsync($"/instance/connect/{instanceName}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro ao obter QR Code: {StatusCode} - {Response}", response.StatusCode, error);
                throw new HttpRequestException($"Falha ao obter QR Code: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<EvolutionQrCodeResponse>();

            return new WhatsAppQrCodeResponse
            {
                QrCodeBase64 = result?.Base64 ?? string.Empty,
                Status = result?.State ?? "pending",
                ExpiresIn = 60
            };
        }

        public async Task<WhatsAppQrCodeResponse> RestartInstanceAsync(string instanceName)
        {
            _logger.LogInformation("Reiniciando instância: {InstanceName}", instanceName);

            var response = await _httpClient.PostAsync($"/instance/restart/{instanceName}", null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro ao reiniciar instância: {StatusCode} - {Response}", response.StatusCode, error);
                throw new HttpRequestException($"Falha ao reiniciar instância: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<EvolutionQrCodeResponse>();

            return new WhatsAppQrCodeResponse
            {
                QrCodeBase64 = result?.Base64 ?? string.Empty,
                Status = result?.State ?? "pending",
                ExpiresIn = 60
            };
        }

        public async Task<WhatsAppConnectionStatus> GetConnectionStatusAsync(string instanceName)
        {
            _logger.LogInformation("Verificando status da conexão: {InstanceName}", instanceName);

            var response = await _httpClient.GetAsync($"/instance/connectionState/{instanceName}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new WhatsAppConnectionStatus { IsConnected = false, State = "not_found" };
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro ao verificar status: {StatusCode} - {Response}",
                    response.StatusCode, error);
                throw new HttpRequestException($"Falha ao verificar status: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<EvolutionConnectionStateResponse>();

            return new WhatsAppConnectionStatus
            {
                InstanceName = instanceName,
                State = result?.State ?? "close",
                IsConnected = result?.State?.ToLower() == "open",
                PhoneNumber = result?.Instance?.Owner
            };
        }

        public async Task DeleteInstanceAsync(string instanceName)
        {
            _logger.LogInformation("Deletando instância: {InstanceName}", instanceName);

            var response = await _httpClient.DeleteAsync($"/instance/delete/{instanceName}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro ao deletar instância: {StatusCode} - {Response}",
                    response.StatusCode, error);
                throw new HttpRequestException($"Falha ao deletar instância: {error}");
            }

            _logger.LogInformation("Instância {InstanceName} deletada com sucesso", instanceName);
        }

        public async Task<string> SendTextMessageAsync(string instanceName, string phoneNumber, string message)
        {
            _logger.LogInformation("Enviando mensagem via instância {InstanceName} para {PhoneNumber}",
                instanceName, phoneNumber);

            var formattedPhone = phoneNumber.Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");

            var request = new
            {
                number = formattedPhone,
                text = message
            };

            var response = await _httpClient.PostAsJsonAsync($"/message/sendText/{instanceName}", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro ao enviar mensagem: {StatusCode} - {Response}",
                    response.StatusCode, error);
                throw new HttpRequestException($"Falha ao enviar mensagem: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<EvolutionSendMessageResponse>();
            return result?.Key?.Id ?? "unknown";
        }

        public async Task<string> SendButtonsMessageAsync(string instanceName, string phoneNumber, string title, string description, string footer, List<WhatsAppButton> buttons)
        {
            _logger.LogInformation("Enviando mensagem com botões via instância {InstanceName} para {PhoneNumber}", instanceName, phoneNumber);

            var formattedPhone = phoneNumber.Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");

            var request = new
            {
                number = formattedPhone,
                title,
                description,
                footer,
                buttons = buttons.Select(b => new
                {
                    type = b.Type,
                    displayText = b.DisplayText,
                    id = b.Id,
                    copyCode = b.CopyCode,
                    url = b.Url,
                    phoneNumber = b.PhoneNumber,
                    currency = b.Currency,
                    name = b.Name,
                    keyType = b.KeyType,
                    key = b.Key
                }).ToList()
            };

            var response = await _httpClient.PostAsJsonAsync($"/message/sendButtons/{instanceName}", request, new System.Text.Json.JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro ao enviar mensagem com botões: {StatusCode} - {Response}", response.StatusCode, error);
                throw new HttpRequestException($"Falha ao enviar mensagem com botões: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<EvolutionSendMessageResponse>();
            return result?.Key?.Id ?? "unknown";
        }
    }

    #region Evolution API Response Models

    internal class EvolutionCreateInstanceResponse
    {
        [JsonPropertyName("instance")]
        public EvolutionInstanceInfo? Instance { get; set; }

        [JsonPropertyName("qrcode")]
        public EvolutionQrCodeInfo? Qrcode { get; set; }
    }

    internal class EvolutionInstanceInfo
    {
        [JsonPropertyName("instanceName")]
        public string? InstanceName { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("owner")]
        public string? Owner { get; set; }
    }

    internal class EvolutionQrCodeInfo
    {
        [JsonPropertyName("base64")]
        public string? Base64 { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }
    }

    internal class EvolutionQrCodeResponse
    {
        [JsonPropertyName("base64")]
        public string? Base64 { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }
    }

    internal class EvolutionConnectionStateResponse
    {
        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("instance")]
        public EvolutionInstanceInfo? Instance { get; set; }
    }

    internal class EvolutionSendMessageResponse
    {
        [JsonPropertyName("key")]
        public EvolutionMessageKey? Key { get; set; }
    }

    internal class EvolutionMessageKey
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("remoteJid")]
        public string? RemoteJid { get; set; }
    }

    #endregion
}
