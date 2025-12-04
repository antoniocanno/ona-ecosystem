using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ona.Auth.Application.Interfaces.Repositories;

namespace Ona.Auth.Infrastructure.Services.Background
{
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24);

        public TokenCleanupService(IServiceProvider serviceProvider)
        {
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
                    // Logar a exceção. 
                }

                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }
    }
}
