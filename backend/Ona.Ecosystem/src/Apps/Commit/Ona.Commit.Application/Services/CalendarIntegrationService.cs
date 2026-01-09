using Ona.Commit.Application.DTOs.Responses;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Interfaces;
using Ona.Commit.Domain.Interfaces.Repositories;
using Ona.Core.Common.Exceptions;

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
                _ => throw new ValidationException("Invalid provider")
            };
        }

        public async Task<CalendarIntegrationResponse> CompleteAuthAsync(CompleteCalendarAuthRequest request)
        {
            var authData = request.Provider switch
            {
                CalendarProvider.Google => await _googleService.ExchangeCodeForTokenAsync(request.Code, request.State),
                CalendarProvider.Outlook => await _outlookService.ExchangeCodeForTokenAsync(request.Code, request.State),
                _ => throw new ValidationException("Invalid provider")
            };

            var integration = await _repository.GetByProfessionalAndProviderAsync(request.ProfessionalId, request.Provider);

            if (integration != null)
            {
                integration.AccessToken = authData.AccessToken;
                integration.EncryptedRefreshToken = authData.EncryptedRefreshToken;
                integration.TokenExpiry = authData.TokenExpiry;
                integration.TokenIssuedAtUtc = authData.TokenIssuedAtUtc;
                integration.Email = authData.Email;
                integration.ExternalEmailAddress = authData.ExternalEmailAddress;
                integration.ExternalCalendarId = authData.ExternalCalendarId;

                _repository.Update(integration);
            }
            else
            {
                integration = authData;
                integration.ProfessionalId = request.ProfessionalId;
                integration.IsActive = true;

                await _repository.CreateAsync(integration);
            }

            await _repository.SaveChangesAsync();
            await _calendarService.SubscribeToNotificationsAsync(request.ProfessionalId);

            return new CalendarIntegrationResponse
            {
                Id = integration.Id,
                Provider = integration.Provider.ToString(),
                IsActive = integration.IsActive,
                Email = integration.Email
            };
        }

        public async Task RemoveIntegrationAsync(Guid professionalId, CalendarProvider provider)
        {
            var integration = await _repository.GetByProfessionalAndProviderAsync(professionalId, provider);

            if (integration == null)
                throw new NotFoundException("Integração não encontrada");

            await _calendarService.UnsubscribeFromNotificationsAsync(professionalId, provider);

            _repository.Remove(integration);
            await _repository.SaveChangesAsync();
        }
    }
}
