using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;
using Ona.Application.Shared.Interfaces.Services;
using Ona.Commit.Application.DTOs;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Interfaces;
using Ona.Core.Common.Exceptions;
using Ona.Core.Interfaces;

namespace Ona.Commit.Infrastructure.Integrations
{
    public class OutlookCalendarService : IOutlookCalendarService
    {
        private readonly ICurrentUser _currentUser;
        private readonly ICurrentTenant _currentTenant;
        private readonly ILogger<OutlookCalendarService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _microsoftTenantId;
        private readonly string _redirectUri;
        private readonly ICryptographyService _cryptoService;
        private readonly ITenantService _tenantService;
        private readonly IDistributedCache _distributedCache;
        private static readonly string[] Scopes = { "Calendars.ReadWrite", "offline_access" };

        public OutlookCalendarService(
            IConfiguration configuration,
            ICryptographyService cryptoService,
            ILogger<OutlookCalendarService> logger,
            ITenantService tenantService,
            ICurrentUser currentUser,
            ICurrentTenant currentTenant,
            IDistributedCache distributedCache)
        {
            _configuration = configuration;
            _cryptoService = cryptoService;
            _logger = logger;
            _tenantService = tenantService;
            _currentUser = currentUser;
            _currentTenant = currentTenant;
            _distributedCache = distributedCache;
            //_clientId = _configuration["Microsoft:ClientId"] ?? throw new InvalidOperationException("Microsoft ClientId não configurado");
            //_clientSecret = _configuration["Microsoft:ClientSecret"] ?? throw new InvalidOperationException("Microsoft ClientSecret não configurado");
            //_microsoftTenantId = _configuration["Microsoft:TenantId"] ?? "common";
            //_redirectUri = _configuration["Microsoft:RedirectUri"] ?? throw new InvalidOperationException("Microsoft RedirectUri não configurado");
        }

        public string GetAuthUrl()
        {
            var stateDto = new StateDto
            {
                UserId = _currentUser.Id.Value,
                TenantId = _currentTenant.Id.Value,
                Nonce = Guid.NewGuid()
            };

            var state = _cryptoService.Encrypt(JsonSerializer.Serialize(stateDto));

            var app = ConfidentialClientApplicationBuilder
                .Create(_clientId)
                .WithClientSecret(_clientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{_microsoftTenantId}"))
                .WithRedirectUri(_redirectUri)
                .Build();

            var authUrl = app.GetAuthorizationRequestUrl(Scopes)
                .WithPrompt(Microsoft.Identity.Client.Prompt.Consent)
                .ExecuteAsync()
                .GetAwaiter()
                .GetResult();

            return $"{authUrl}&state={Uri.EscapeDataString(state)}";
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

                var app = ConfidentialClientApplicationBuilder
                    .Create(_clientId)
                    .WithClientSecret(_clientSecret)
                    .WithAuthority(new Uri($"https://login.microsoftonline.com/{_microsoftTenantId}"))
                    .WithRedirectUri(_redirectUri)
                    .Build();

                byte[]? tokenCacheData = null;

                app.UserTokenCache.SetAfterAccess(args =>
                {
                    if (args.HasStateChanged)
                    {
                        tokenCacheData = args.TokenCache.SerializeMsalV3();
                    }
                });

                var result = await app.AcquireTokenByAuthorizationCode(Scopes, code)
                    .ExecuteAsync();

                var homeAccountId = result.Account.HomeAccountId.Identifier;

                if (tokenCacheData != null)
                {
                    var cacheKey = $"msal_cache_{homeAccountId}";
                    await _distributedCache.SetAsync(cacheKey, tokenCacheData, new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(90)
                    });
                }

                _logger.LogInformation("Token do Outlook Calendar obtido com sucesso. Expira em: {Expiry}", result.ExpiresOn);

                return new CalendarIntegration
                {
                    AccessToken = result.AccessToken,
                    EncryptedRefreshToken = _cryptoService.Encrypt(homeAccountId),
                    TokenExpiry = result.ExpiresOn.UtcDateTime,
                    Provider = CalendarProvider.Outlook,
                    TokenIssuedAtUtc = DateTime.UtcNow,
                    ExternalEmailAddress = result.Account.Username
                };
            }
            catch (MsalException ex)
            {
                _logger.LogError(ex, "Erro MSAL ao trocar código por token do Outlook: {ErrorCode}", ex.ErrorCode);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao trocar código por token do Outlook");
                throw;
            }
        }

        public async Task<string> CreateEventAsync(CalendarIntegration integration, Appointment appointment)
        {
            try
            {
                var graphClient = await GetGraphClientAsync(integration);
                var timeZone = await GetTenantTimeZoneAsync(integration.TenantId);

                var newEvent = new Event
                {
                    Subject = appointment.Summary,
                    Body = new ItemBody
                    {
                        ContentType = BodyType.Text,
                        Content = appointment.Description
                    },
                    Start = new DateTimeTimeZone
                    {
                        DateTime = appointment.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                        TimeZone = timeZone
                    },
                    End = new DateTimeTimeZone
                    {
                        DateTime = appointment.EndDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                        TimeZone = timeZone
                    },
                    ReminderMinutesBeforeStart = 30,
                    IsReminderOn = true,
                    Extensions = new List<Extension>
                    {
                        new OpenTypeExtension
                        {
                            ExtensionName = "com.ona.commit.metadata",
                            AdditionalData = new Dictionary<string, object>
                            {
                                { "appointmentId", appointment.Id },
                                { "tenantId", integration.TenantId }
                            }
                        }
                    }
                };

                var createdEvent = await graphClient.Me.Calendar.Events
                    .PostAsync(newEvent);

                _logger.LogInformation(
                    "Evento criado no Outlook Calendar. EventId: {EventId}, AppointmentId: {AppointmentId}",
                    createdEvent?.Id,
                    appointment.Id
                );

                return createdEvent?.Id ?? throw new InvalidOperationException("Evento criado mas ID não retornado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar evento no Outlook Calendar para Appointment {AppointmentId}", appointment.Id);
                throw;
            }
        }

        public async Task UpdateEventAsync(CalendarIntegration integration, Appointment appointment, string externalEventId)
        {
            try
            {
                var graphClient = await GetGraphClientAsync(integration);
                var timeZone = await GetTenantTimeZoneAsync(integration.TenantId);

                var updatedEvent = new Event
                {
                    Subject = appointment.Summary,
                    Body = new ItemBody
                    {
                        ContentType = BodyType.Text,
                        Content = appointment.Description
                    },
                    Start = new DateTimeTimeZone
                    {
                        DateTime = appointment.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                        TimeZone = timeZone
                    },
                    End = new DateTimeTimeZone
                    {
                        DateTime = appointment.EndDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                        TimeZone = timeZone
                    }
                };

                await graphClient.Me.Calendar.Events[externalEventId]
                    .PatchAsync(updatedEvent);

                _logger.LogInformation("Evento atualizado no Outlook Calendar. EventId: {EventId}", externalEventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar evento {EventId} no Outlook Calendar", externalEventId);
                throw;
            }
        }

        public async Task DeleteEventAsync(CalendarIntegration integration, string externalEventId)
        {
            try
            {
                var graphClient = await GetGraphClientAsync(integration);
                await graphClient.Me.Calendar.Events[externalEventId]
                    .DeleteAsync();

                _logger.LogInformation("Evento deletado do Outlook Calendar. EventId: {EventId}", externalEventId);
            }
            catch (Microsoft.Graph.Models.ODataErrors.ODataError ex) when (ex.ResponseStatusCode == 404)
            {
                _logger.LogWarning("Evento {EventId} não encontrado no Outlook Calendar (já deletado?)", externalEventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar evento {EventId} do Outlook Calendar", externalEventId);
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

        private async Task<string> RefreshAccessTokenAsync(CalendarIntegration integration)
        {
            try
            {
                var accountId = _cryptoService.Decrypt(integration.EncryptedRefreshToken);
                var cacheKey = $"msal_cache_{accountId}";

                var app = ConfidentialClientApplicationBuilder
                    .Create(_clientId)
                    .WithClientSecret(_clientSecret)
                    .WithAuthority(new Uri($"https://login.microsoftonline.com/{_microsoftTenantId}"))
                    .WithRedirectUri(_redirectUri)
                    .Build();

                app.UserTokenCache.SetBeforeAccess(args =>
                {
                    var data = _distributedCache.Get(cacheKey);
                    if (data != null)
                    {
                        args.TokenCache.DeserializeMsalV3(data);
                    }
                });

                app.UserTokenCache.SetAfterAccess(args =>
                {
                    if (args.HasStateChanged)
                    {
                        _distributedCache.Set(cacheKey, args.TokenCache.SerializeMsalV3(), new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(90)
                        });
                    }
                });

                var account = await app.GetAccountAsync(accountId);

                if (account == null)
                {
                    throw new InvalidOperationException("Conta Microsoft não encontrada. Reautenticação necessária.");
                }

                var result = await app.AcquireTokenSilent(Scopes, account)
                    .ExecuteAsync();

                integration.AccessToken = result.AccessToken;
                integration.TokenExpiry = result.ExpiresOn.UtcDateTime;

                _logger.LogInformation("Token do Outlook Calendar renovado com sucesso");

                return integration.AccessToken;
            }
            catch (MsalUiRequiredException ex)
            {
                _logger.LogError(ex, "Reautenticação necessária para renovar token do Outlook");
                throw new UnauthorizedAccessException("Reautenticação necessária. Por favor, reconecte sua conta Microsoft.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao renovar token do Outlook Calendar");
                throw;
            }
        }

        public async Task SubscribeToNotificationsAsync(CalendarIntegration integration, string webhookUrl)
        {
            try
            {
                var graphClient = await GetGraphClientAsync(integration);

                var subscription = new Subscription
                {
                    ChangeType = "created,updated,deleted",
                    NotificationUrl = webhookUrl,
                    Resource = "me/calendar/events",
                    ExpirationDateTime = DateTimeOffset.UtcNow.AddMinutes(4200), // Limite max é 4230 min (~2.9 dias), usando 4200 por segurança
                    ClientState = Guid.NewGuid().ToString()
                };

                var createdSubscription = await graphClient.Subscriptions
                    .PostAsync(subscription);

                integration.ExternalChannelId = createdSubscription?.Id;
                integration.ExternalResourceId = createdSubscription?.ClientState;

                _logger.LogInformation(
                    "Webhook registrado no Outlook Calendar. SubscriptionId: {SubscriptionId}",
                    createdSubscription?.Id
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar webhook no Outlook Calendar");
                throw;
            }
        }

        public async Task<IEnumerable<ExternalEventDto>> GetChangedEventsAsync(CalendarIntegration integration)
        {
            try
            {
                var graphClient = await GetGraphClientAsync(integration);
                var events = new List<ExternalEventDto>();

                Microsoft.Graph.Me.Calendar.Events.Delta.DeltaGetResponse? collection;

                if (!string.IsNullOrEmpty(integration.SyncToken))
                {
                    var deltaUrl = integration.SyncToken.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                        ? integration.SyncToken
                        : $"https://graph.microsoft.com/v1.0/me/calendar/events/delta?$deltatoken={integration.SyncToken}";

                    var deltaRequest = new Microsoft.Graph.Me.Calendar.Events.Delta.DeltaRequestBuilder(deltaUrl, graphClient.RequestAdapter);
                    collection = await deltaRequest.GetAsDeltaGetResponseAsync();
                }
                else
                {
                    collection = await graphClient.Me.Calendar.Events.Delta.GetAsDeltaGetResponseAsync(config =>
                    {
                        config.QueryParameters.Top = 100;
                        config.QueryParameters.Orderby = new[] { "start/dateTime" };

                        var startTime = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-ddTHH:mm:ssZ");
                        config.QueryParameters.StartDateTime = startTime;
                    });
                }

                if (collection?.Value != null)
                {
                    foreach (var eventItem in collection.Value)
                    {
                        events.Add(MapToExternalEventDto(eventItem));
                    }

                    var pageIterator = PageIterator<Event, Microsoft.Graph.Me.Calendar.Events.Delta.DeltaGetResponse>
                        .CreatePageIterator(graphClient, collection, (evt) =>
                        {
                            events.Add(MapToExternalEventDto(evt));
                            return true;
                        });

                    await pageIterator.IterateAsync();

                    if (!string.IsNullOrEmpty(collection.OdataDeltaLink))
                    {
                        integration.SyncToken = collection.OdataDeltaLink;
                    }
                }

                _logger.LogInformation("Sincronizados {Count} eventos do Outlook Calendar", events.Count);

                return events;
            }
            catch (Microsoft.Graph.Models.ODataErrors.ODataError ex) when (ex.ResponseStatusCode == 410)
            {
                _logger.LogWarning("DeltaToken expirado para integração {Id}. Realizando sync completo.", integration.Id);
                integration.SyncToken = null;
                return await GetChangedEventsAsync(integration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar eventos alterados do Outlook Calendar");
                throw;
            }
        }

        private async Task<GraphServiceClient> GetGraphClientAsync(CalendarIntegration integration)
        {
            var accessToken = await GetValidAccessTokenAsync(integration);

            var authProvider = new BaseBearerTokenAuthenticationProvider(new TokenProvider(accessToken));

            return new GraphServiceClient(authProvider);
        }

        private async Task<string> GetTenantTimeZoneAsync(Guid tenantId)
        {
            var tenant = await _tenantService.GetByIdAsync(tenantId);
            var ianaTimeZone = tenant?.TimeZone ?? "America/Sao_Paulo";

            try
            {
                if (TimeZoneInfo.TryConvertIanaIdToWindowsId(ianaTimeZone, out string? windowsTimeZone))
                {
                    return windowsTimeZone;
                }
            }
            catch
            {
            }

            return "E. South America Standard Time";

        }

        private ExternalEventDto MapToExternalEventDto(Event outlookEvent)
        {
            DateTimeOffset start = DateTimeOffset.MinValue;
            DateTimeOffset end = DateTimeOffset.MinValue;

            bool isDeleted = false;
            if (outlookEvent.AdditionalData != null && outlookEvent.AdditionalData.ContainsKey("@removed"))
            {
                isDeleted = true;
            }

            if (outlookEvent.Start != null && !string.IsNullOrEmpty(outlookEvent.Start.DateTime))
            {
                if (DateTimeOffset.TryParse(outlookEvent.Start.DateTime, out var startDt))
                {
                    start = startDt;
                }
            }

            if (outlookEvent.End != null && !string.IsNullOrEmpty(outlookEvent.End.DateTime))
            {
                if (DateTimeOffset.TryParse(outlookEvent.End.DateTime, out var endDt))
                {
                    end = endDt;
                }
            }

            return new ExternalEventDto
            {
                Id = outlookEvent.Id ?? "",
                Summary = outlookEvent.Subject ?? "",
                Description = outlookEvent.Body?.Content ?? "",
                Start = start,
                End = end,
                Status = isDeleted ? "cancelled" : (outlookEvent.IsCancelled == true ? "cancelled" : "confirmed"),
                Updated = outlookEvent.LastModifiedDateTime ?? DateTime.UtcNow
            };
        }

        private class TokenProvider : IAccessTokenProvider
        {
            private readonly string _accessToken;

            public TokenProvider(string accessToken)
            {
                _accessToken = accessToken;
            }

            public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(_accessToken);
            }

            public AllowedHostsValidator AllowedHostsValidator => new AllowedHostsValidator();
        }
    }
}