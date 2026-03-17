using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ona.Commit.Application.DTOs.Responses;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Interfaces;
using Ona.Commit.Domain.Interfaces.Repositories;
using Ona.Core.Common.Exceptions;
using Ona.Core.Interfaces;
using Ona.Core.Tenant;
using System.Text.Json;

namespace Ona.Commit.Infrastructure.Integrations
{
    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly ICurrentUser _currentUser;
        private readonly ICurrentTenant _currentTenant;
        private readonly ILogger<GoogleCalendarService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;
        private readonly ICryptographyService _cryptoService;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICalendarIntegrationRepository _repository;
        private static readonly string[] Scopes = { CalendarService.Scope.Calendar };

        public GoogleCalendarService(
            IConfiguration configuration,
            ICryptographyService cryptoService,
            ILogger<GoogleCalendarService> logger,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            ICurrentTenant currentTenant,
            ICalendarIntegrationRepository repository)
        {
            _configuration = configuration;
            _cryptoService = cryptoService;
            _logger = logger;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _currentTenant = currentTenant;
            _repository = repository;
            _clientId = _configuration["Google:ClientId"] ?? "";
            _clientSecret = _configuration["Google:ClientSecret"] ?? "";
            _redirectUri = _configuration["Google:RedirectUri"] ?? "";
        }

        public string GetAuthUrl()
        {
            var flow = new OfflineGoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _clientId,
                    ClientSecret = _clientSecret
                },
                Scopes = Scopes
            });

            var stateDto = new StateDto
            {
                UserId = _currentUser.Id.Value,
                TenantId = _currentTenant.Id.Value,
                Nonce = Guid.NewGuid()
            };

            var state = _cryptoService.Encrypt(JsonSerializer.Serialize(stateDto));

            var url = flow.CreateAuthorizationCodeRequest(_redirectUri);
            url.State = state;
            return url.Build().AbsoluteUri;
        }

        public async Task<CalendarIntegration> ExchangeCodeForTokenAsync(string code, string state)
        {
            try
            {
                if (string.IsNullOrEmpty(state))
                    throw new ValidationException("Estado do OAuth2 ausente.");

                var decryptedState = _cryptoService.Decrypt(state);
                var stateDto = JsonSerializer.Deserialize<StateDto>(decryptedState);

                if (stateDto == null)
                    throw new ValidationException("Formato inválido do estado do OAuth2.");

                if (stateDto.TenantId != _currentTenant.Id)
                    throw new UnauthorizedAccessException("Incompatibilidade de tenant no estado OAuth2.");

                if (stateDto.UserId != _currentUser.Id)
                    throw new UnauthorizedAccessException("Incompatibilidade de usuário no estado OAuth2.");

                var flow = new OfflineGoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = _clientId,
                        ClientSecret = _clientSecret
                    },
                    Scopes = Scopes
                });

                var tokenResponse = await flow.ExchangeCodeForTokenAsync(
                    userId: _currentUser.Id.ToString(),
                    code: code,
                    redirectUri: _redirectUri,
                    CancellationToken.None
                );

                var expiry = tokenResponse.IssuedUtc.AddSeconds(tokenResponse.ExpiresInSeconds ?? 3600);

                _logger.LogInformation("Token do Google Calendar obtido com sucesso via SDK. Expira em: {Expiry}", expiry);

                return new CalendarIntegration
                {
                    AccessToken = tokenResponse.AccessToken,
                    EncryptedRefreshToken = _cryptoService.Encrypt(tokenResponse.RefreshToken),
                    TokenExpiry = expiry,
                    Provider = CalendarProvider.Google,
                    TokenIssuedAtUtc = tokenResponse.IssuedUtc
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao trocar código por token do Google usando SDK");
                throw;
            }
        }

        public async Task<string> CreateEventAsync(CalendarIntegration integration, Appointment appointment)
        {
            try
            {
                var service = await GetCalendarServiceAsync(integration);

                var timeZone = await GetTenantTimeZoneAsync(integration.TenantId);

                var newEvent = new Event
                {
                    Summary = appointment.Summary,
                    Description = appointment.Description,
                    Start = new EventDateTime
                    {
                        DateTimeDateTimeOffset = appointment.StartDate,
                        TimeZone = timeZone
                    },
                    End = new EventDateTime
                    {
                        DateTimeDateTimeOffset = appointment.EndDate,
                        TimeZone = timeZone
                    },
                    Reminders = new Event.RemindersData
                    {
                        UseDefault = false,
                        Overrides =
                        [
                            new() { Method = "popup", Minutes = 30 }
                        ]
                    },
                    ExtendedProperties = new Event.ExtendedPropertiesData
                    {
                        Private__ = new Dictionary<string, string>
                        {
                            { "appointmentId", appointment.Id.ToString() },
                            { "tenantId", integration.TenantId.ToString() }
                        }
                    }
                };

                var request = service.Events.Insert(newEvent, "primary");
                var createdEvent = await request.ExecuteAsync();

                _logger.LogInformation(
                    "Evento criado no Google Calendar. EventId: {EventId}, AppointmentId: {AppointmentId}",
                    createdEvent.Id,
                    appointment.Id
                );

                return createdEvent.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar evento no Google Calendar para Appointment {AppointmentId}", appointment.Id);
                throw;
            }
        }

        public async Task UpdateEventAsync(CalendarIntegration integration, Appointment appointment, string externalEventId)
        {
            try
            {
                var service = await GetCalendarServiceAsync(integration);

                var eventToUpdate = await service.Events.Get("primary", externalEventId).ExecuteAsync();

                var timeZone = await GetTenantTimeZoneAsync(integration.TenantId);

                eventToUpdate.Summary = appointment.Summary;
                eventToUpdate.Description = appointment.Description;
                eventToUpdate.Start = new EventDateTime
                {
                    DateTimeDateTimeOffset = appointment.StartDate,
                    TimeZone = timeZone
                };
                eventToUpdate.End = new EventDateTime
                {
                    DateTimeDateTimeOffset = appointment.EndDate,
                    TimeZone = timeZone
                };

                await service.Events.Update(eventToUpdate, "primary", externalEventId).ExecuteAsync();

                _logger.LogInformation("Evento atualizado no Google Calendar. EventId: {EventId}", externalEventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar evento {EventId} no Google Calendar", externalEventId);
                throw;
            }
        }

        public async Task DeleteEventAsync(CalendarIntegration integration, string externalEventId)
        {
            try
            {
                var service = await GetCalendarServiceAsync(integration);
                await service.Events.Delete("primary", externalEventId).ExecuteAsync();

                _logger.LogInformation("Evento deletado do Google Calendar. EventId: {EventId}", externalEventId);
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Evento {EventId} não encontrado no Google Calendar (já deletado?)", externalEventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar evento {EventId} do Google Calendar", externalEventId);
                throw;
            }
        }

        public async Task<string> GetValidAccessTokenAsync(CalendarIntegration integration)
        {
            if (!string.IsNullOrEmpty(integration.AccessToken) &&
                integration.TokenExpiry > DateTime.UtcNow.AddMinutes(5))
            {
                return integration.AccessToken;
            }

            return await RefreshAccessTokenAsync(integration);
        }

        public async Task<bool> RefreshTokenIfNeededAsync(CalendarIntegration integration)
        {
            if (integration.TokenExpiry.HasValue &&
                integration.TokenExpiry.Value <= DateTime.UtcNow.AddMinutes(15))
            {
                _logger.LogInformation("Token do Google Calendar para {ProfessionalId} expira em breve ({Expiry}). Renovando...",
                    integration.ProfessionalId, integration.TokenExpiry);

                await RefreshAccessTokenAsync(integration);
                return true;
            }

            return false;
        }

        private async Task<string> RefreshAccessTokenAsync(CalendarIntegration integration)
        {
            try
            {
                var refreshToken = _cryptoService.Decrypt(integration.EncryptedRefreshToken);

                var flow = new OfflineGoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = _clientId,
                        ClientSecret = _clientSecret
                    },
                    Scopes = Scopes
                });

                var newToken = await flow.RefreshTokenAsync("user", refreshToken, CancellationToken.None);

                integration.AccessToken = newToken.AccessToken;
                integration.TokenExpiry = newToken.IssuedUtc.AddSeconds(newToken.ExpiresInSeconds ?? 3600);

                _logger.LogInformation("Token do Google Calendar renovado com sucesso via SDK");

                return integration.AccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao renovar token do Google Calendar usando SDK");
                throw;
            }
        }

        public async Task<IEnumerable<ExternalEventDto>> GetEventsAsync(CalendarIntegration integration)
        {
            try
            {
                var service = await GetCalendarServiceAsync(integration);

                var request = service.Events.List("primary");
                request.TimeMinDateTimeOffset = DateTime.UtcNow;
                request.TimeMaxDateTimeOffset = DateTime.UtcNow.AddDays(30);
                request.SingleEvents = true;
                request.MaxResults = 100;
                request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

                Events events = await request.ExecuteAsync();
                if (events.Items == null) return [];

                _logger.LogInformation("Sincronizados {Count} eventos do Google Calendar", events.Items.Count);

                var googleEventIds = events.Items.Select(e => e.Id).ToList();

                var existingIds = await _repository.GetExistingExternalIdsAsync(googleEventIds, CalendarProvider.Google);

                return events.Items.Select(e =>
                {
                    return new ExternalEventDto
                    {
                        Id = e.Id,
                        Summary = e.Summary,
                        Start = e.Start.DateTimeDateTimeOffset ?? DateTime.Parse(e.Start.Date),
                        End = e.End.DateTimeDateTimeOffset ?? DateTime.Parse(e.End.Date),
                        AlreadyImported = existingIds.Contains(e.Id)
                    };
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar eventos do Google Calendar.");
                throw;
            }
        }

        public async Task SubscribeToNotificationsAsync(CalendarIntegration integration, string webhookUrl)
        {
            try
            {
                var service = await GetCalendarServiceAsync(integration);

                var channelId = Guid.NewGuid().ToString();
                var channel = new Channel
                {
                    Id = channelId,
                    Type = "web_hook",
                    Address = webhookUrl,
                    Expiration = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeMilliseconds()
                };

                var request = service.Events.Watch(channel, "primary");
                var response = await request.ExecuteAsync();

                integration.ExternalChannelId = response.Id;
                integration.ExternalResourceId = response.ResourceId;

                _logger.LogInformation(
                    "Webhook registrado no Google Calendar. ChannelId: {ChannelId}, ResourceId: {ResourceId}",
                    response.Id,
                    response.ResourceId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar webhook no Google Calendar");
                throw;
            }
        }

        public async Task UnsubscribeFromNotificationsAsync(CalendarIntegration integration)
        {
            try
            {
                if (string.IsNullOrEmpty(integration.ExternalChannelId) || string.IsNullOrEmpty(integration.ExternalResourceId))
                {
                    return;
                }

                var service = await GetCalendarServiceAsync(integration);
                var channel = new Channel
                {
                    Id = integration.ExternalChannelId,
                    ResourceId = integration.ExternalResourceId
                };

                await service.Channels.Stop(channel).ExecuteAsync();

                _logger.LogInformation(
                    "Webhook cancelado no Google Calendar. ChannelId: {ChannelId}, ResourceId: {ResourceId}",
                    integration.ExternalChannelId,
                    integration.ExternalResourceId
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao cancelar webhook no Google Calendar (o canal já pode ter expirado)");
            }
        }

        public async Task<IEnumerable<ExternalEventDto>> GetChangedEventsAsync(CalendarIntegration integration)
        {
            try
            {
                var service = await GetCalendarServiceAsync(integration);
                var events = new List<ExternalEventDto>();

                var request = service.Events.List("primary");
                request.MaxResults = 100;
                request.SingleEvents = true;
                request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

                if (!string.IsNullOrEmpty(integration.SyncToken))
                {
                    request.SyncToken = integration.SyncToken;
                }
                else
                {
                    request.TimeMinDateTimeOffset = DateTime.UtcNow.AddDays(-30);
                }

                Events eventsFeed;
                do
                {
                    eventsFeed = await request.ExecuteAsync();

                    if (eventsFeed.Items != null)
                    {
                        foreach (var eventItem in eventsFeed.Items)
                        {
                            events.Add(MapToExternalEventDto(eventItem));
                        }
                    }

                    request.PageToken = eventsFeed.NextPageToken;
                } while (!string.IsNullOrEmpty(eventsFeed.NextPageToken));

                if (!string.IsNullOrEmpty(eventsFeed.NextSyncToken))
                {
                    integration.SyncToken = eventsFeed.NextSyncToken;
                }

                _logger.LogInformation("Sincronizados {Count} eventos do Google Calendar", events.Count);

                return events;
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.Gone)
            {
                _logger.LogWarning("SyncToken expirado para integração {Id}. Realizando sync completo.", integration.Id);
                integration.SyncToken = null;
                return await GetChangedEventsAsync(integration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar eventos alterados do Google Calendar");
                throw;
            }
        }

        private async Task<CalendarService> GetCalendarServiceAsync(CalendarIntegration integration)
        {
            var accessToken = await GetValidAccessTokenAsync(integration);
            var refreshToken = _cryptoService.Decrypt(integration.EncryptedRefreshToken);

            var tokenResponse = new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                IssuedUtc = integration.TokenIssuedAtUtc,
                ExpiresInSeconds = (long)(integration.TokenExpiry - DateTime.UtcNow).Value.TotalSeconds
            };

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = _clientId,
                    ClientSecret = _clientSecret
                },
                Scopes = Scopes,
                DataStore = new NullDataStore()
            });

            var credential = new UserCredential(flow, "user", tokenResponse);

            return new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "OnaCommit"
            });
        }

        private async Task<string> GetTenantTimeZoneAsync(Guid tenantId)
        {
            var tenant = await _tenantProvider.GetAsync(tenantId);
            return tenant?.TimeZone ?? "America/Sao_Paulo";
        }

        private ExternalEventDto MapToExternalEventDto(Event googleEvent)
        {
            return new ExternalEventDto
            {
                Id = googleEvent.Id,
                Summary = googleEvent.Summary ?? "",
                Description = googleEvent.Description ?? "",
                Start = googleEvent.Start?.DateTimeDateTimeOffset ?? DateTimeOffset.MinValue,
                End = googleEvent.End?.DateTimeDateTimeOffset ?? DateTimeOffset.MinValue,
                Status = googleEvent.Status,
                Updated = googleEvent.UpdatedDateTimeOffset ?? DateTime.UtcNow
            };
        }

        private class NullDataStore : IDataStore
        {
            public Task StoreAsync<T>(string key, T value) => Task.CompletedTask;
            public Task DeleteAsync<T>(string key) => Task.CompletedTask;
            public Task<T> GetAsync<T>(string key) => Task.FromResult(default(T));
            public Task ClearAsync() => Task.CompletedTask;
        }

        private class OfflineGoogleAuthorizationCodeFlow : GoogleAuthorizationCodeFlow
        {
            public OfflineGoogleAuthorizationCodeFlow(Initializer initializer) : base(initializer) { }

            public override AuthorizationCodeRequestUrl CreateAuthorizationCodeRequest(string redirectUri)
            {
                return new GoogleAuthorizationCodeRequestUrl(new Uri(AuthorizationServerUrl))
                {
                    ClientId = ClientSecrets.ClientId,
                    Scope = string.Join(" ", Scopes),
                    RedirectUri = redirectUri,
                    AccessType = "offline",
                    Prompt = "consent"
                };
            }
        }
    }
}
