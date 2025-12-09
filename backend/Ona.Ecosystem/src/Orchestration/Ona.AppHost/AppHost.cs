var builder = DistributedApplication.CreateBuilder(args);

var jwtSecret = builder.AddParameter("JwtSecret", secret: true); 
var password = builder.AddParameter("pg-password", secret: true);

var postgres = builder.AddPostgres("postgres", password: password)
    .WithDataVolume("ona-postgres-data-v2")
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent);

var authDb = postgres.AddDatabase("auth-db");
var quoteDb = postgres.AddDatabase("quote-db");

var redis = builder.AddRedis("cache");

var authApi = builder.AddProject<Projects.Ona_Auth_API>("ona-auth-api")
                     .WithEnvironment("SSO:Secret", jwtSecret)
                     .WithReference(authDb)
                     .WaitFor(postgres)
                     .WithReference(redis);

var quoteApi = builder.AddProject<Projects.Ona_Quote_API>("ona-quote-api")
                       .WithReference(authApi)
                       .WithEnvironment("SSO:Secret", jwtSecret)
                       .WithReference(quoteDb)
                       .WaitFor(postgres)
                       .WithReference(redis);

builder.AddProject<Projects.Ona_Commit_API>("ona-commit-api");

builder.Build().Run();
