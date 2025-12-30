using Ona.Commit.Application.DTOs;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Interfaces;
using Ona.Commit.Domain.Interfaces.Repositories;

namespace Ona.Commit.Application.Services
{
    public class CalendarIntegrationService : ICalendarIntegrationService
    {
        private readonly ICalendarIntegrationRepository _repository;
        private readonly IGoogleCalendarService _googleService;
        private readonly IOutlookCalendarService _outlookService;
        private readonly ICryptographyService _cryptoService;
        private readonly ICalendarService _calendarService;

        public CalendarIntegrationService(
            ICalendarIntegrationRepository repository,
            IGoogleCalendarService googleService,
            IOutlookCalendarService outlookService,
            ICryptographyService cryptoService,
            ICalendarService calendarService)
        {
            _repository = repository;
            _googleService = googleService;
            _outlookService = outlookService;
            _cryptoService = cryptoService;
            _calendarService = calendarService;
        }

        public string GetAuthUrl(InitiateCalendarAuthRequest request)
        {
            return request.Provider switch
            {
                CalendarProvider.Google => _googleService.GetAuthUrl(),
                CalendarProvider.Outlook => _outlookService.GetAuthUrl(),
                _ => throw new ArgumentException("Invalid provider")
            };
        }

        public async Task<CalendarIntegrationResponse> CompleteAuthAsync(CompleteCalendarAuthRequest request)
        {
            string accessToken, refreshToken;
            DateTime expiry;

            if (request.Provider == CalendarProvider.Google)
            {
                (accessToken, refreshToken, expiry) = await _googleService.ExchangeCodeForTokenAsync(request.Code);
            }
            else if (request.Provider == CalendarProvider.Outlook)
            {
                (accessToken, refreshToken, expiry) = await _outlookService.ExchangeCodeForTokenAsync(request.Code);
            }
            else
            {
                throw new ArgumentException("Invalid provider");
            }

            var existing = await _repository.GetByCustomerAndProviderAsync(request.CustomerId, request.Provider);
            if (existing != null)
            {
                existing.AccessToken = accessToken;
                existing.EncryptedRefreshToken = _cryptoService.Encrypt(refreshToken);
                existing.TokenExpiry = expiry;
                _repository.Update(existing);
                await _repository.SaveChangesAsync();

                // Trigger Sync Subscription on Re-Auth as well? Probably good idea.
                await _calendarService.SubscribeToNotificationsAsync(request.CustomerId);

                return new CalendarIntegrationResponse
                {
                    Id = existing.Id,
                    Provider = existing.Provider.ToString(),
                    IsActive = existing.IsActive,
                    Email = existing.Email
                };
            }

            // Create new integration
            var integration = new CalendarIntegration
            {
                CustomerId = request.CustomerId,
                Provider = request.Provider,
                AccessToken = accessToken,
                EncryptedRefreshToken = _cryptoService.Encrypt(refreshToken),
                TokenExpiry = expiry,
                IsActive = true
            };

            await _repository.CreateAsync(integration);
            await _repository.SaveChangesAsync();

            // Trigger Inbound Sync Subscription
            await _calendarService.SubscribeToNotificationsAsync(request.CustomerId);

            return new CalendarIntegrationResponse
            {
                Id = integration.Id,
                Provider = integration.Provider.ToString(),
                IsActive = integration.IsActive,
                Email = integration.Email
            };
        }
    }
}
