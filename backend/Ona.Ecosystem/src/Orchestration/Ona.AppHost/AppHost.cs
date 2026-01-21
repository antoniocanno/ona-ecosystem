using Ona.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

// --- Parâmetros e Segredos ---
var jwtSecret = builder.AddParameter("JwtSecret", secret: true);
var jwtIssuer = builder.AddParameter("JwtIssuer");
var jwtAudience = builder.AddParameter("JwtAudience");
var internalApiKey = builder.AddParameter("InternalApiKey", secret: true);
var password = builder.AddParameter("pg-password", secret: true);
var cryptographyKey = builder.AddParameter("CryptographyKey", secret: true);

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

var redisPass = builder.AddParameter("redis-pass", "redispass");
var redis = builder.AddRedis("cache", password: redisPass);
var rabbitUser = builder.AddParameter("rabbit-user", "admin");
var rabbitPass = builder.AddParameter("rabbit-pass", "admin");
var rabbitMq = builder.AddRabbitMQ("rabbitmq", userName: rabbitUser, password: rabbitPass)
                      .WithImageTag("4.0-management")
                      .WithManagementPlugin();

// --- Container da Evolution API ---
var evolution = builder.AddEvolutionApi(postgres, password, redis, rabbitMq);
var evolutionApi = evolution.Container;
var evolutionApiKey = evolution.ApiKey;

// --- Projetos .NET ---
var authApi = builder.AddProject<Projects.Ona_Auth_API>("ona-auth-api")
                     .WithEnvironment("JwtSettings:Secret", jwtSecret)
                     .WithEnvironment("JwtSettings:Issuer", jwtIssuer)
                     .WithEnvironment("JwtSettings:Audience", jwtAudience)
                     .WithEnvironment("Auth:InternalApiKey", internalApiKey)
                     .WithEnvironment("Cryptography:Key", cryptographyKey)
                     .WithReference(authDb)
                     .WithReference(redis)
                     .WithReference(rabbitMq)
                     .WaitFor(postgres)
                     .WaitFor(rabbitMq)
                     .WaitFor(postgres);

builder.AddProject<Projects.Ona_Commit_Worker_Hangfire>("ona-commit-worker-hangfire")
                    .WithReference(commitDb)
                    .WithEnvironment("WhatsApp:Evolution:ApiUrl", evolutionApi.GetEndpoint("api"))
                    .WithEnvironment("WhatsApp:Evolution:ApiKey", evolutionApiKey)
                    .WithEnvironment("Auth:InternalApiKey", internalApiKey)
                    .WithEnvironment("Cryptography:Key", cryptographyKey)
                    .WithReference(rabbitMq)
                    .WaitFor(postgres)
                    .WaitFor(rabbitMq)
                    .WaitFor(evolutionApi);

builder.AddProject<Projects.Ona_Commit_API>("ona-commit-api")
                       .WithReference(authApi)
                       .WithEnvironment("JwtSettings:Secret", jwtSecret)
                       .WithEnvironment("JwtSettings:Issuer", jwtIssuer)
                       .WithEnvironment("JwtSettings:Audience", jwtAudience)
                       .WithEnvironment("Auth:InternalApiKey", internalApiKey)
                       .WithEnvironment("Cryptography:Key", cryptographyKey)
                       .WithReference(commitDb)
                       .WithEnvironment("WhatsApp:Evolution:ApiUrl", evolutionApi.GetEndpoint("api"))
                       .WithEnvironment("WhatsApp:Evolution:ApiKey", evolutionApiKey)
                       .WithReference(redis)
                       .WithReference(rabbitMq)
                       .WaitFor(postgres)
                       .WaitFor(rabbitMq);

builder.Build().Run();