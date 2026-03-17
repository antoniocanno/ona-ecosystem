using Microsoft.EntityFrameworkCore;
using Ona.Auth.Infrastructure.Data;
using Ona.Core.Common.Exceptions;
using Polly;
using Polly.Retry;

namespace Ona.Auth.API.Extensions
{
    public static class MigrationExtensions
    {
        public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            var pipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 10,
                    Delay = TimeSpan.FromSeconds(2),
                    BackoffType = DelayBackoffType.Constant,
                    OnRetry = args =>
                    {
                        logger.LogWarning("Database unavailable. Retrying in 2s... (Attempt {Attempt}/{Max})",
                            args.AttemptNumber, 10);
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();

            try
            {
                await pipeline.ExecuteAsync(async ct =>
                {
                    logger.LogInformation("Checking database connectivity...");

                    if (!await db.Database.CanConnectAsync(ct))
                        throw new ConfigurationException("Database connection failed.");

                    var pending = (await db.Database.GetPendingMigrationsAsync(ct)).ToList();

                    if (pending.Count == 0)
                    {
                        logger.LogInformation("No pending migrations.");
                        return;
                    }

                    logger.LogInformation("Applying migrations: {M}", string.Join(", ", pending));
                    await db.Database.MigrateAsync(ct);
                    logger.LogInformation("Migrations applied successfully.");
                });
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Fatal error running migrations.");
                throw;
            }
        }
    }
}
