using Ona.Auth.Infrastructure.Data;
using Ona.Commit.Infrastructure.Data;
using Ona.Core.Interfaces;
using Ona.MigrationService;
using Ona.ServiceDefaults.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ICurrentUser, CurrentUser>();
builder.Services.AddSingleton<ICurrentTenant, CurrentTenant>();

builder.AddNpgsqlDbContext<AuthDbContext>("auth-db", settings =>
{
    settings.DisableRetry = false;
    settings.CommandTimeout = 300;
});

builder.AddNpgsqlDbContext<CommitDbContext>("commit-db", settings =>
{
    settings.DisableRetry = false;
    settings.CommandTimeout = 300;
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
