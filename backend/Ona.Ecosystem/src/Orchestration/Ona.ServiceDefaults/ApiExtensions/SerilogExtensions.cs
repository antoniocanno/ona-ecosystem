using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;

namespace Ona.ServiceDefaults.ApiExtensions
{
    public static class SerilogExtensions
    {
        public static IHostApplicationBuilder AddCustomSerilog(this IHostApplicationBuilder builder)
        {
            builder.Services.AddSerilog((services, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(builder.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithProperty("ApplicationName", builder.Environment.ApplicationName);

                loggerConfiguration.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <{SourceContext}>{NewLine}{Exception}",
                    theme: AnsiConsoleTheme.Code
                );

                if (builder.Environment.IsProduction())
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
