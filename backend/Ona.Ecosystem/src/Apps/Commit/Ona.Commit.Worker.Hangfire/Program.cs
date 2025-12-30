using Hangfire;
using Hangfire.PostgreSql;
using Ona.Commit.Application.Interfaces.Repositories;
using Ona.Commit.Application.Interfaces.Services;
using Ona.Commit.Domain.Interfaces.Repositories;
using Ona.Commit.Domain.Interfaces.Workers;
using Ona.Commit.Infrastructure.Integrations;
using Ona.Commit.Infrastructure.Repositories;
using Ona.Commit.Infrastructure.Services;
using Ona.Commit.Worker.Hangfire.Jobs;
using Ona.ServiceDefaults;

namespace Ona.Commit.Worker.Hangfire;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.AddServiceDefaults();

        builder.Services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("commit-db"))));

        builder.Services.AddHangfireServer();

        // Register Application/Infrastructure Services
        builder.Services.AddScoped<ICalendarSyncWorker, CalendarSyncWorker>();

        // Repositories
        builder.Services.AddScoped<ICalendarIntegrationRepository, CalendarIntegrationRepository>();
        builder.Services.AddScoped<IExternalCalendarEventMappingRepository, ExternalCalendarEventMappingRepository>();
        builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

        // Calendar Services
        builder.Services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();
        builder.Services.AddScoped<IOutlookCalendarService, OutlookCalendarService>();

        // Security Services
        builder.Services.AddScoped<Domain.Interfaces.ICryptographyService, CryptographyService>();

        var host = builder.Build();
        host.Run();
    }
}
