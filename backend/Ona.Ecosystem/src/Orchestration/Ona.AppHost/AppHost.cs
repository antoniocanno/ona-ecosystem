var builder = DistributedApplication.CreateBuilder(args);

var jwtSecret = builder.AddParameter("JwtSecret", secret: true);
var jwtIssuer = builder.AddParameter("JwtIssuer");
var jwtAudience = builder.AddParameter("JwtAudience");
var password = builder.AddParameter("pg-password", secret: true);

var postgres = builder.AddPostgres("postgres", password: password)
    .WithDataVolume("ona-postgres-data-v2")
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent);

var authDb = postgres.AddDatabase("auth-db");
var commitDb = postgres.AddDatabase("commit-db");

var redis = builder.AddRedis("cache");

var authApi = builder.AddProject<Projects.Ona_Auth_API>("ona-auth-api")
                     .WithEnvironment("JwtSettings:Secret", jwtSecret)
                     .WithEnvironment("JwtSettings:Issuer", jwtIssuer)
                     .WithEnvironment("JwtSettings:Audience", jwtAudience)
                     .WithReference(authDb)
                     .WaitFor(postgres)
                     .WithReference(redis);

var commitApi = builder.AddProject<Projects.Ona_Commit_API>("ona-commit-api")
                       .WithReference(authApi)
                       .WithEnvironment("JwtSettings:Secret", jwtSecret)
                       .WithEnvironment("JwtSettings:Issuer", jwtIssuer)
                       .WithEnvironment("JwtSettings:Audience", jwtAudience)
                       .WithReference(commitDb)
                       .WaitFor(postgres)
                       .WithReference(redis);

builder.Build().Run();
