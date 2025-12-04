using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Ona.Auth.API.Extensions
{
    public static class SerilogExtensions
    {
        public static IHostBuilder UseSerilogConfiguration(this IHostBuilder builder)
        {
            builder.UseSerilog((hostingContext, services, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(hostingContext.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithProperty("ApplicationName", hostingContext.HostingEnvironment.ApplicationName);

                loggerConfiguration.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <{SourceContext}>{NewLine}{Exception}",
                    theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code
                );

                if (hostingContext.HostingEnvironment.IsProduction())
                {
                    loggerConfiguration.WriteTo.File(
                        formatter: new CompactJsonFormatter(),
                        path: "logs/log-.json",
                        rollingInterval: RollingInterval.Day,
                        restrictedToMinimumLevel: LogEventLevel.Information
                    );
                }
            });

            return builder;
        }
    }
}
