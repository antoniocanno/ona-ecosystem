var builder = DistributedApplication.CreateBuilder(args);

// --- Parâmetros e Segredos ---
var jwtSecret = builder.AddParameter("JwtSecret", secret: true);
var jwtIssuer = builder.AddParameter("JwtIssuer");
var jwtAudience = builder.AddParameter("JwtAudience");
var password = builder.AddParameter("pg-password", secret: true);
var evolutionApiKey = builder.AddParameter("EvolutionApiKey", secret: true);

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

// --- TRUQUE DE MESTRE: Montando a URI do Postgres para Node.js ---
var evolutionPostgresUri = ReferenceExpression.Create(
    $"postgresql://postgres:{password}@postgres:5432/evolution-db?schema=public");

var evolutionRedisUri = ReferenceExpression.Create(
    $"redis://cache:6379");

// --- Container da Evolution API ---
var evolutionApi = builder.AddContainer("evolution-api", "atendai/evolution-api")
    .WithEnvironment("AUTHENTICATION_TYPE", "apikey")
    .WithEnvironment("AUTHENTICATION_API_KEY", evolutionApiKey)
    .WithEnvironment("SERVER_URL", "http://localhost:8080")
    .WithEnvironment("TZ", "America/Sao_Paulo")

    // Configuração de Banco de Dados (Postgres)
    .WithEnvironment("DATABASE_ENABLED", "true")
    .WithEnvironment("DATABASE_PROVIDER", "postgresql")
    .WithEnvironment("DATABASE_CONNECTION_URI", evolutionPostgresUri)
    .WithEnvironment("DATABASE_CONNECTION_CLIENT_NAME", "evolution_exchange")

    // Configuração de Cache (Redis) - CRÍTICO PARA PERFORMANCE
    .WithEnvironment("CACHE_REDIS_ENABLED", "true")
    .WithEnvironment("CACHE_REDIS_URI", evolutionRedisUri)
    .WithEnvironment("CACHE_REDIS_DB", "0")
    .WithEnvironment("CACHE_REDIS_PREFIX", "evolution:")
    .WithEnvironment("REDIS_ENABLED", "true")
    .WithEnvironment("REDIS_URI", evolutionRedisUri)

    // Flags de Persistência (Mantive as suas)
    .WithEnvironment("DATABASE_SAVE_DATA_INSTANCE", "true")
    .WithEnvironment("DATABASE_SAVE_DATA_NEW_MESSAGE", "true")
    .WithEnvironment("DATABASE_SAVE_MESSAGE_UPDATE", "true")
    .WithEnvironment("DATABASE_SAVE_DATA_CONTACTS", "true")
    .WithEnvironment("DATABASE_SAVE_DATA_CHATS", "true")
    .WithEnvironment("DATABASE_SAVE_DATA_LABELS", "true")
    .WithEnvironment("DATABASE_SAVE_DATA_HISTORIC", "true")

    .WithHttpEndpoint(targetPort: 8080, name: "api")
    .WaitFor(postgres)
    .WaitFor(redis);

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