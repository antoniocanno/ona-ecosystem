using Microsoft.EntityFrameworkCore;
using Ona.Commit.Infrastructure.Data;
using Polly;
using Polly.Retry;

namespace Ona.Commit.API.Extensions
{
    public static class MigrationExtensions
    {
        public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CommitDbContext>();
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
                        throw new Exception("Database connection failed.");

                    var pending = (await db.Database.GetPendingMigrationsAsync(ct)).ToList();

                    if (!pending.Any())
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
