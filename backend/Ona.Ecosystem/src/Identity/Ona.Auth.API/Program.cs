
using Microsoft.EntityFrameworkCore;
using Ona.Auth.API.Extensions;
using Ona.Auth.Application.Extensions;
using Ona.Auth.Infrastructure.Data;
using Ona.Auth.Infrastructure.Extensions;
using Ona.ServiceDefaults;
using Polly;
using Polly.Retry;

namespace Ona.Auth.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.AddNpgsqlDbContext<AuthDbContext>("auth-db", settings =>
            {
                settings.DisableRetry = false;
                settings.CommandTimeout = 300;
            });

            builder.AddServiceDefaults();
            builder.AddRedisDistributedCache();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSwaggerDocumentation();

            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddApplication(builder.Configuration);

            builder.Services.AddJwtAuthentication(builder.Configuration);

            builder.Host.UseSerilogConfiguration();

            var app = builder.Build();

            app.MapDefaultEndpoints();

            if (app.Environment.IsDevelopment())
            {
                await ApplyMigrationsAutomatically(app);

                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCustomErrorHandling();
            app.UseEmailVerificationMiddleware();
            app.UseRateLimitMiddleware();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static async Task ApplyMigrationsAutomatically(WebApplication app)
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
                        logger.LogWarning("Database unavailable. Retrying in 2s... (Attempt {AttemptNumber}/{MaxRetries})",
                            args.AttemptNumber, 10);
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();

            try
            {
                await pipeline.ExecuteAsync(async cancellationToken =>
                {
                    logger.LogInformation("Checking database connectivity...");

                    if (!await db.Database.CanConnectAsync(cancellationToken))
                    {
                        throw new Exception("Database connection failed (CanConnect returned false).");
                    }

                    var pendingMigrations = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

                    if (pendingMigrations.Count == 0)
                    {
                        logger.LogInformation("Database is up to date. No migrations pending.");
                        return;
                    }

                    logger.LogInformation("Found {Count} pending migration(s): {Migrations}",
                        pendingMigrations.Count,
                        string.Join(", ", pendingMigrations));

                    logger.LogInformation("Applying migrations...");

                    await db.Database.MigrateAsync(cancellationToken);

                    logger.LogInformation("Migrations applied successfully!");
                });
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Fatal error: Could not connect to the database or apply migrations after multiple attempts.");
                throw;
            }
        }
    }
}
