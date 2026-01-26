using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ona.Commit.Application.Handlers;
using Ona.Commit.Application.Interfaces.Repositories;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Application.Services;
using Ona.Commit.Domain.Events;
using Ona.Commit.Domain.Interfaces;
using Ona.Commit.Domain.Interfaces.Gateways;
using Ona.Commit.Domain.Interfaces.Repositories;
using Ona.Commit.Domain.Interfaces.Workers;
using Ona.Commit.Infrastructure.Gateways.Evolution;
using Ona.Commit.Infrastructure.Integrations;
using Ona.Commit.Infrastructure.Jobs;
using Ona.Commit.Infrastructure.MultiTenancy;
using Ona.Commit.Infrastructure.Repositories;
using Ona.Commit.Infrastructure.Services;
using Ona.Core.Common.Events;
using Ona.Core.Interfaces;
using Ona.Core.Tenant;

namespace Ona.Commit.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IProfessionalRepository, ProfessionalRepository>();
        services.AddScoped<ICalendarIntegrationRepository, CalendarIntegrationRepository>();
        services.AddScoped<IExternalCalendarEventMappingRepository, ExternalCalendarEventMappingRepository>();

        // WhatsApp Repositories
        services.AddScoped<ITenantWhatsAppConfigRepository, TenantWhatsAppConfigRepository>();
        services.AddScoped<IMessageInteractionLogRepository, MessageInteractionLogRepository>();
        services.AddScoped<IMessageTemplateRepository, MessageTemplateRepository>();

        // Application Services
        services.AddScoped<IAppointmentAppService, AppointmentAppService>();
        services.AddScoped<IProfessionalAppService, ProfessionalAppService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ICustomerAppService, CustomerAppService>();
        services.AddScoped<ICalendarIntegrationService, CalendarIntegrationService>();
        services.AddScoped<ICalendarService, CalendarService>();
        services.AddScoped<IWhatsAppAppService, WhatsAppAppService>();
        services.AddScoped<IProxyServerAppService, ProxyServerAppService>();

        // Integration Services
        services.AddTransient<AuthenticationDelegatingHandler>();

        services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();
        services.AddScoped<IOutlookCalendarService, OutlookCalendarService>();

        // WhatsApp Services
        services.AddScoped<ITemplateMessageBuilder, TemplateMessageBuilder>();


        // WhatsApp Gateway (Evolution API)
        services.AddScoped<IWhatsAppGateway, EvolutionWhatsAppGateway>();

        services.AddHttpClient("Ona.Auth", client =>
        {
            client.BaseAddress = new Uri("http://ona-auth-api");
            var internalKey = configuration["Auth:InternalApiKey"];
            if (!string.IsNullOrEmpty(internalKey))
            {
                client.DefaultRequestHeaders.Add("X-Internal-Api-Key", internalKey);
            }
        });

        services.AddStackExchangeRedisCache(options =>
        {
            var redisConnection = configuration.GetConnectionString("redis");
            options.Configuration = redisConnection;
            options.InstanceName = "Ona_Commit_";
        });

        services.AddSingleton<EvolutionMessageDispatcher>();

        services.AddHttpClient<EvolutionWhatsAppGateway>((serviceProvider, client) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var url = configuration["WhatsApp:Evolution:ApiUrl"]
                      ?? throw new InvalidOperationException("URL da Evolution API não configurada.");

            client.BaseAddress = new Uri(url);

            var apiKey = configuration["WhatsApp:Evolution:ApiKey"]
                         ?? throw new InvalidOperationException("API Key da Evolution não configurada.");

            client.DefaultRequestHeaders.Add("apikey", apiKey);

            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Gateways / Repositories        
        services.AddScoped<ITenantWhatsAppConfigRepository, TenantWhatsAppConfigRepository>();
        services.AddScoped<IMessageInteractionLogRepository, MessageInteractionLogRepository>();
        services.AddScoped<IWhatsAppNumberVerificationRepository, WhatsAppNumberVerificationRepository>();
        services.AddScoped<IProxyServerRepository, ProxyServerRepository>();
        services.AddScoped<IWhatsAppGateway, HumanizedWhatsAppGateway>();

        services.AddScoped<IProxyResourceManager, ProxyResourceManager>();

        // Worker / Jobs
        services.AddScoped<ICalendarSyncWorker, CalendarSyncWorker>();
        services.AddScoped<ICalendarTokenRefreshWorker, CalendarTokenRefreshWorker>();
        services.AddScoped<IAppointmentReminderScheduler, AppointmentReminderScheduler>();
        services.AddScoped<IWhatsAppReminderJob, EvolutionWhatsAppReminderJob>();

        // Security / Infrastructure Services
        services.AddSingleton<ICryptographyService>(_ => new CryptographyService(configuration));

        services.AddScoped<ITenantProvider, HttpTenantProvider>();

        // Domain Events
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IDomainEventHandler<AppointmentCancelledByPatientEvent>, AppointmentCancelledHandler>();

        // Notifications
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IOperatorAlertRepository, OperatorAlertRepository>();
        services.AddScoped<IAlertAppService, AlertAppService>();

        return services;
    }
}
