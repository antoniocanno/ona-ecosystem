using Hangfire;
using Hangfire.PostgreSql;
using Ona.Commit.Infrastructure.Data;
using Ona.Commit.Infrastructure.Extensions;
using Ona.Commit.Worker.Hangfire.Extensions;
using Ona.ServiceDefaults;

namespace Ona.Commit.Worker.Hangfire;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.AddNpgsqlDbContext<CommitDbContext>("commit-db");

        builder.AddWorkerServiceDefaults();

        builder.Services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("commit-db"))));

        builder.Services.AddHangfireServer();

        // Register Application/Infrastructure Services (including ICalendarSyncWorker)
        builder.Services.AddInfrastructure(builder.Configuration);

        var host = builder.Build();

        host.RegisterHangfireJobs();

        host.Run();
    }
}
