var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("pg-password", secret: true);

var postgres = builder.AddPostgres("postgres", password: password)
    .WithDataVolume("ona-postgres-data-v2")
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent);

var authDb = postgres.AddDatabase("auth-db");

var redis = builder.AddRedis("cache");

var authApi = builder.AddProject<Projects.Ona_Auth_API>("auth-api")
                     .WithHttpEndpoint(port: 5001, name: "api")
                     .WithReference(authDb)
                     .WaitFor(postgres)
                     .WithReference(redis);

builder.Build().Run();
