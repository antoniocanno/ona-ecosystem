using Microsoft.Extensions.Logging;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Enums;
using Ona.Commit.Domain.Interfaces.Gateways;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ona.Commit.Infrastructure.Gateways.Evolution
{
    /// <summary>
    /// Cliente HTTP para a Evolution API.
    /// Utilizado internamente pelo HumanizedWhatsAppGateway.
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

        public async Task<WhatsAppInstanceResponse> CreateInstanceAsync(Guid tenantId, string instanceName, ProxyServer? proxy = null)
        {
            _logger.LogInformation("Criando instância Evolution API: {InstanceName} para tenant {TenantId}. Proxy: {Proxy}", instanceName, tenantId, proxy?.Host ?? "Nenhum");

            var request = new
            {
                instanceName,
                qrcode = true,
                integration = "WHATSAPP-BAILEYS",
                proxyHost = proxy?.Host,
                proxyPort = proxy?.Port,
                proxyProtocol = proxy?.Protocol,
                proxyUsername = proxy?.Username,
                proxyPassword = proxy?.Password,
            };

            var response = await _httpClient.PostAsJsonAsync("/instance/create", request, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

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
                State = result?.Instance?.State ?? "close",
                IsConnected = result?.Instance?.State?.ToLower() == "open",
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

        public async Task SendPresenceAsync(string instanceName, string phoneNumber, string presence = "composing", int delay = 1200)
        {
            _logger.LogInformation("Enviando presença {Presence} via instância {InstanceName} para {PhoneNumber}", presence, instanceName, phoneNumber);

            var formattedPhone = phoneNumber.Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");

            var request = new
            {
                number = formattedPhone,
                delay,
                presence
            };

            var response = await _httpClient.PostAsJsonAsync($"/chat/sendPresence/{instanceName}", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Falha ao enviar presença: {StatusCode} - {Response}", response.StatusCode, error);
            }
        }

        public async Task<bool> CheckNumberAsync(string instanceName, string phoneNumber)
        {
            _logger.LogInformation("Verificando existência do número {PhoneNumber} via instância {InstanceName}", phoneNumber, instanceName);

            var formattedPhone = phoneNumber.Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");

            var request = new
            {
                numbers = new[] { formattedPhone }
            };

            var response = await _httpClient.PostAsJsonAsync($"/chat/whatsappNumbers/{instanceName}", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Falha ao verificar número: {StatusCode} - {Response}", response.StatusCode, error);
                throw new HttpRequestException($"Falha ao verificar número: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<List<EvolutionNumberCheckResponse>>();
            return result?.FirstOrDefault()?.Exists ?? false;
        }

        public async Task SetProxyAsync(string instanceName, ProxyServer proxy)
        {
            _logger.LogInformation("Definindo proxy para instância {InstanceName}: {Proxy}", instanceName, proxy.Host);

            var request = new
            {
                enabled = true,
                host = proxy.Host,
                port = proxy.Port,
                protocol = proxy.Protocol,
                username = proxy.Username,
                password = proxy.Password
            };

            var response = await _httpClient.PostAsJsonAsync($"/proxy/set/{instanceName}", request, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro ao definir proxy na Evolution: {StatusCode} - {Response}", response.StatusCode, error);
                throw new HttpRequestException($"Falha ao configurar proxy: {error}");
            }
        }

        public async Task SetRabbitMqConfigAsync(string instanceName)
        {
            _logger.LogInformation("Configurando RabbitMQ para instância {InstanceName}", instanceName);

            var request = new
            {
                enabled = true,
                events = new[] { "APPLICATION_STARTUP", "QRCODE_UPDATED", "CONNECTION_UPDATE", "MESSAGES_UPSERT" }
            };

            var response = await _httpClient.PostAsJsonAsync($"/rabbitmq/set/{instanceName}", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro ao configurar RabbitMQ na Evolution: {StatusCode} - {Response}", response.StatusCode, error);
                throw new HttpRequestException($"Falha ao configurar RabbitMQ: {error}");
            }
        }

        public async Task<NotificationStatus> GetMessageStatusAsync(string instanceName, string phoneNumber, string messageId)
        {
            _logger.LogInformation("Buscando status da mensagem {MessageId} na instância {InstanceName}", messageId, instanceName);

            var formattedPhone = phoneNumber.Replace("+", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");
            var remoteJid = formattedPhone.Contains("@") ? formattedPhone : $"{formattedPhone}@s.whatsapp.net";

            var requestBody = new
            {
                where = new
                {
                    id = messageId,
                    remoteJid = remoteJid,
                    fromMe = true
                },
                offset = 10,
                page = 1,
            };

            var response = await _httpClient.PostAsJsonAsync($"/chat/findStatusMessage/{instanceName}", requestBody, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Falha ao buscar status da mensagem: {StatusCode} - {Response}", response.StatusCode, error);

                if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    return NotificationStatus.Scheduled;
                }

                throw new HttpRequestException($"Falha ao buscar status da mensagem: {error}");
            }

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            string? statusStr = null;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                {
                    if (item.TryGetProperty("key", out var key) && key.TryGetProperty("id", out var idProp) && idProp.GetString() == messageId)
                    {
                        if (item.TryGetProperty("status", out var s))
                        {
                            statusStr = s.GetString();
                            break;
                        }
                    }
                }
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("key", out var key) && key.TryGetProperty("id", out var idProp) && idProp.GetString() == messageId)
                {
                    if (root.TryGetProperty("status", out var s))
                    {
                        statusStr = s.GetString();
                    }
                }
            }

            if (string.IsNullOrEmpty(statusStr))
            {
                _logger.LogWarning("Status para mensagem {MessageId} não encontrado no retorno da API", messageId);
                return NotificationStatus.Scheduled;
            }

            return MapEvolutionStatusToDomain(statusStr);
        }

        private NotificationStatus MapEvolutionStatusToDomain(string status)
        {
            return status?.ToUpper() switch
            {
                "PENDING" => NotificationStatus.Sent,
                "SERVER_ACK" => NotificationStatus.Sent,
                "DELIVERY_ACK" => NotificationStatus.Delivered,
                "READ" => NotificationStatus.Read,
                "PLAYED" => NotificationStatus.Read,
                "ERROR" => NotificationStatus.Failed,
                _ => NotificationStatus.Sent
            };
        }
    }

    #region Evolution API Response Models

    internal class EvolutionNumberCheckResponse
    {
        [JsonPropertyName("exists")]
        public bool Exists { get; set; }
    }

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
