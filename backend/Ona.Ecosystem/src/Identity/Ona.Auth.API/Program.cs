using Ona.Auth.API.Extensions;
using Ona.Auth.Application.Extensions;
using Ona.Auth.Infrastructure.Data;
using Ona.Auth.Infrastructure.Extensions;
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

            builder.AddServiceDefaults();
            builder.AddRedisDistributedCache();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddApplication(builder.Configuration);

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

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
