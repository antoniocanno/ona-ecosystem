using Microsoft.Extensions.Logging;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Entities;
using Ona.Commit.Domain.Interfaces.Repositories;
using Ona.Commit.Domain.Interfaces.Workers;

namespace Ona.Commit.Infrastructure.Jobs
{
    public class CalendarTokenRefreshWorker : ICalendarTokenRefreshWorker
    {
        private readonly ICalendarIntegrationRepository _repository;
        private readonly IGoogleCalendarService _googleService;
        private readonly IOutlookCalendarService _outlookService;
        private readonly ILogger<CalendarTokenRefreshWorker> _logger;

        public CalendarTokenRefreshWorker(
            ICalendarIntegrationRepository repository,
            IGoogleCalendarService googleService,
            IOutlookCalendarService outlookService,
            ILogger<CalendarTokenRefreshWorker> logger)
        {
            _repository = repository;
            _googleService = googleService;
            _outlookService = outlookService;
            _logger = logger;
        }

        public async Task RefreshExpiringTokensAsync()
        {
            _logger.LogInformation("Iniciando Verificação de renovação de tokens de calendário...");

            var integrations = await _repository.GetAllActiveAsync();
            int renewedCount = 0;

            foreach (var integration in integrations)
            {
                try
                {
                    bool renewed = integration.Provider switch
                    {
                        CalendarProvider.Google => await _googleService.RefreshTokenIfNeededAsync(integration),
                        CalendarProvider.Outlook => await _outlookService.RefreshTokenIfNeededAsync(integration),
                        _ => false
                    };

                    if (renewed)
                    {
                        _repository.Update(integration);
                        renewedCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao renovar token para integração {IntegrationId} ({Provider})",
                        integration.Id, integration.Provider);
                }
            }

            if (renewedCount > 0)
            {
                await _repository.SaveChangesAsync();
                _logger.LogInformation("{Count} tokens renovados com sucesso.", renewedCount);
            }

            _logger.LogInformation("Verificação de renovação de tokens finalizada.");
        }
    }
}
