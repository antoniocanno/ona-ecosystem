using Hangfire;
using Hangfire.PostgreSql;
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

        var host = builder.Build();
        host.Run();
    }
}
