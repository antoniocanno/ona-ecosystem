using Ona.Commit.API.Extensions;
using Ona.Commit.Infrastructure.Data;
using Ona.Commit.Infrastructure.Extensions;
using Ona.ServiceDefaults;

namespace Ona.Commit.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.AddNpgsqlDbContext<CommitDbContext>("commit-db", settings =>
            {
                settings.DisableRetry = false;
                settings.CommandTimeout = 300;
            });

            builder.AddServiceDefaults();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddInfrastructure(builder.Configuration);

            var app = builder.Build();

            app.MapDefaultEndpoints();

            // Configure the HTTP request pipeline.
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
