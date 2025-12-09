using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ona.Auth.Application.Interfaces.Repositories;

namespace Ona.Auth.Infrastructure.Services.Background
{
    public class TokenCleanupService : BackgroundService
    {
        private readonly ILogger<TokenCleanupService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24);

        public TokenCleanupService(
            ILogger<TokenCleanupService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var cleanupRepositories = scope.ServiceProvider
                        .GetRequiredService<IEnumerable<ICleanupableTokenRepository>>();

                    foreach (var repository in cleanupRepositories)
                    {
                        await repository.CleanupExpiredTokensAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao limpar tokens expirados");
                }

                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }
    }
}
