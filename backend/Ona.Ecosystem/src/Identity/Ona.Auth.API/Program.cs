using Ona.Auth.API.Extensions;
using Ona.Auth.API.Middlewares;
using Ona.Auth.Application.Extensions;
using Ona.Auth.Application.Services;
using Ona.Auth.Infrastructure.Data;
using Ona.Auth.Infrastructure.Extensions;
using Ona.Core.Tenant;
using Ona.ServiceDefaults;

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

            builder.AddApiServiceDefaults();
            builder.AddRedisDistributedCache();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddApplication(builder.Configuration);

            builder.Services.AddScoped<ITenantProvider, DbTenantProvider>();

            var app = builder.Build();

            app.MapDefaultEndpoints();

            if (app.Environment.IsDevelopment())
            {
                await app.ApplyDatabaseMigrationsAsync();

                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.AddServiceDefaults();

            app.UseHttpsRedirection();

            app.UseMiddleware<InternalApiKeyMiddleware>();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
