using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Ona.Auth.Infrastructure.Data;
using Ona.Commit.Infrastructure.Data;

namespace Ona.MigrationService;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<Worker> logger) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = ActivitySource.StartActivity("Migrating databases", ActivityKind.Client);

        try
        {
            await using var scope = serviceProvider.CreateAsyncScope();

            var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            await MigrateAsync(authDb, stoppingToken);

            var commitDb = scope.ServiceProvider.GetRequiredService<CommitDbContext>();
            await MigrateAsync(commitDb, stoppingToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            logger.LogCritical(ex, "Fatal error running migrations.");
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    private async Task MigrateAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        var contextName = dbContext.GetType().Name;
        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            logger.LogInformation("Checking database connectivity for {Context}...", contextName);

            if (!await dbContext.Database.CanConnectAsync(cancellationToken))
            {
                throw new InvalidOperationException($"Database connection failed for {contextName}.");
            }

            var pending = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

            if (pending.Count == 0)
            {
                logger.LogInformation("No pending migrations for {Context}.", contextName);
                return;
            }

            logger.LogInformation("Applying {Count} migration(s) for {Context}: {Migrations}",
                pending.Count, contextName, string.Join(", ", pending));

            await dbContext.Database.MigrateAsync(cancellationToken);

            logger.LogInformation("Migrations applied for {Context}.", contextName);
        });
    }
}
