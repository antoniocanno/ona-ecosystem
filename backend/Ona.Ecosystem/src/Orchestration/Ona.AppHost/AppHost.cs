using Ona.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

// --- Parâmetros e Segredos ---
var jwtSecret = builder.AddParameter("JwtSecret", secret: true);
var jwtIssuer = builder.AddParameter("JwtIssuer");
var jwtAudience = builder.AddParameter("JwtAudience");
var password = builder.AddParameter("pg-password", secret: true);

// --- Infraestrutura (Postgres e Redis) ---
var postgres = builder.AddPostgres("postgres", password: password)
    .WithDataVolume("ona-postgres-data-v2")
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment("TZ", "America/Sao_Paulo");

// Definição dos bancos lógicos
var authDb = postgres.AddDatabase("auth-db");
var commitDb = postgres.AddDatabase("commit-db");
var evolutionDb = postgres.AddDatabase("evolution-db");

var redis = builder.AddRedis("cache");

// --- Container da Evolution API ---
var evolution = builder.AddEvolutionApi(postgres, password, redis);
var evolutionApi = evolution.Container;
var evolutionApiKey = evolution.ApiKey;

// --- Projetos .NET ---
var authApi = builder.AddProject<Projects.Ona_Auth_API>("ona-auth-api")
                     .WithEnvironment("JwtSettings:Secret", jwtSecret)
                     .WithEnvironment("JwtSettings:Issuer", jwtIssuer)
                     .WithEnvironment("JwtSettings:Audience", jwtAudience)
                     .WithReference(authDb)
                     .WaitFor(postgres)
                     .WithReference(redis);

builder.AddProject<Projects.Ona_Commit_Worker_Hangfire>("ona-commit-worker-hangfire")
                    .WithReference(commitDb)
                    .WithEnvironment("WhatsApp__Evolution__ApiUrl", evolutionApi.GetEndpoint("api"))
                    .WithEnvironment("WhatsApp__Evolution__ApiKey", evolutionApiKey)
                    .WaitFor(postgres)
                    .WaitFor(evolutionApi);

builder.AddProject<Projects.Ona_Commit_API>("ona-commit-api")
                       .WithReference(authApi)
                       .WithEnvironment("JwtSettings:Secret", jwtSecret)
                       .WithEnvironment("JwtSettings:Issuer", jwtIssuer)
                       .WithEnvironment("JwtSettings:Audience", jwtAudience)
                       .WithReference(commitDb)
                       .WithEnvironment("WhatsApp__Evolution__ApiUrl", evolutionApi.GetEndpoint("api"))
                       .WithEnvironment("WhatsApp__Evolution__ApiKey", evolutionApiKey)
                       .WaitFor(postgres)
                       .WithReference(redis);

builder.Build().Run();