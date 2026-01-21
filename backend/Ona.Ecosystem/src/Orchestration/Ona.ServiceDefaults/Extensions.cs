using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ona.Core.Interfaces;
using Ona.Core.Tenant;
using Ona.Infrastructure.Shared.Integrations;
using Ona.ServiceDefaults.ApiExtensions;
using Ona.ServiceDefaults.Services;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Ona.ServiceDefaults;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    public static TBuilder AddApiServiceDefaults<TBuilder>(
        this TBuilder builder,
        Action<IBusRegistrationConfigurator>? configureMassTransit = null,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? configureRabbitMq = null) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddCommonServiceDefaults(configureMassTransit, configureRabbitMq);

        builder.Services.AddSwaggerDocumentation();
        builder.Services.AddJwtAuthentication(builder.Configuration);

        builder.ConfigureRoute();

        return builder;
    }

    public static TBuilder AddWorkerServiceDefaults<TBuilder>(
        this TBuilder builder,
        Action<IBusRegistrationConfigurator>? configureMassTransit = null,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? configureRabbitMq = null) where TBuilder : IHostApplicationBuilder
    {
        return builder.AddCommonServiceDefaults(configureMassTransit, configureRabbitMq);
    }

    private static TBuilder AddCommonServiceDefaults<TBuilder>(
        this TBuilder builder,
        Action<IBusRegistrationConfigurator>? configureMassTransit = null,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? configureRabbitMq = null) where TBuilder : IHostApplicationBuilder
    {
        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddMemoryCache();
        builder.Services.AddHttpContextAccessor();

        builder.AddTenantServices();
        builder.AddMessagingServices(configureMassTransit, configureRabbitMq);
        builder.AddCustomSerilog();

        return builder;
    }

    private static void AddTenantServices<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddSingleton<ICurrentUser, CurrentUser>();
        builder.Services.AddSingleton<ICurrentTenant, CurrentTenant>();
        builder.Services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();

        builder.Services.AddTransient<InternalApiKeyHandler>();

        builder.Services.AddHttpClient<ITenantProvider, TenantHttpClient>(client =>
        {
            client.BaseAddress = new Uri("http://ona-auth-api");
        })
        .AddHttpMessageHandler<InternalApiKeyHandler>()
        .AddServiceDiscovery()
        .AddStandardResilienceHandler();
    }

    private static void AddMessagingServices<TBuilder>(
        this TBuilder builder,
        Action<IBusRegistrationConfigurator>? configureMassTransit = null,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? configureRabbitMq = null) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumer<TenantUpdatedConsumer>();

            configureMassTransit?.Invoke(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                var connectionString = builder.Configuration.GetConnectionString("rabbitmq");
                if (!string.IsNullOrEmpty(connectionString))
                {
                    cfg.Host(connectionString);
                }

                configureRabbitMq?.Invoke(context, cfg);

                cfg.ConfigureEndpoints(context);
            });
        });
    }

    public static WebApplication AddServiceDefaults(this WebApplication app)
    {
        app.UseCustomErrorHandling();
        app.UseEmailVerificationMiddleware();
        app.UseRateLimitMiddleware();
        app.UseAuthentication();
        app.UseTenantMiddleware();
        return app;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(options =>
                        // Exclude health check requests from tracing
                        options.Filter = context =>
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                    )
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        return builder.AddOpenTelemetryExporters();
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks(HealthEndpointPath);

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }

    public static TBuilder ConfigureRoute<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
        });

        return builder;
    }
}
