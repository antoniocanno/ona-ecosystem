namespace Ona.AppHost;

public static class EvolutionApiExtensions
{
    public record EvolutionApiResource(IResourceBuilder<ContainerResource> Container, IResourceBuilder<ParameterResource> ApiKey);

    public static EvolutionApiResource AddEvolutionApi(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<IResourceWithConnectionString> postgres,
        IResourceBuilder<ParameterResource> pgPassword,
        IResourceBuilder<RedisResource> redis)
    {
        // --- Parâmetros ---
        var evolutionApiKey = builder.AddParameter("EvolutionApiKey", secret: true);
        var evolutionDbEnabled = builder.AddParameter("Evolution-DatabaseEnabled");
        var evolutionDbProvider = builder.AddParameter("Evolution-DatabaseProvider");
        var evolutionDbClientName = builder.AddParameter("Evolution-DatabaseConnectionClientName");
        var evolutionSaveInstance = builder.AddParameter("Evolution-SaveDataInstance");
        var evolutionSaveNewMessage = builder.AddParameter("Evolution-SaveDataNewMessage");
        var evolutionSaveMessageUpdate = builder.AddParameter("Evolution-SaveMessageUpdate");
        var evolutionSaveContacts = builder.AddParameter("Evolution-SaveDataContacts");
        var evolutionSaveChats = builder.AddParameter("Evolution-SaveDataChats");
        var evolutionSaveLabels = builder.AddParameter("Evolution-SaveDataLabels");
        var evolutionSaveHistoric = builder.AddParameter("Evolution-SaveDataHistoric");
        var evolutionTimeZone = builder.AddParameter("Evolution-TimeZone");

        // --- URIs de Conexão ---
        var evolutionPostgresUri = ReferenceExpression.Create(
            $"postgresql://postgres:{pgPassword}@postgres:5432/evolution-db?schema=public");

        var evolutionRedisUri = ReferenceExpression.Create(
            $"redis://cache:6379");

        // --- Container da Evolution API ---
        var container = builder.AddContainer("evolution-api", "atendai/evolution-api")
            .WithEnvironment("AUTHENTICATION_TYPE", "apikey")
            .WithEnvironment("AUTHENTICATION_API_KEY", evolutionApiKey)
            .WithEnvironment("SERVER_URL", "http://localhost:8080")
            .WithEnvironment("TZ", evolutionTimeZone)

            // Configuração de Banco de Dados (Postgres)
            .WithEnvironment("DATABASE_ENABLED", evolutionDbEnabled)
            .WithEnvironment("DATABASE_PROVIDER", evolutionDbProvider)
            .WithEnvironment("DATABASE_CONNECTION_URI", evolutionPostgresUri)
            .WithEnvironment("DATABASE_CONNECTION_CLIENT_NAME", evolutionDbClientName)

            // Configuração de Cache (Redis)
            .WithEnvironment("CACHE_REDIS_ENABLED", "true")
            .WithEnvironment("CACHE_REDIS_URI", evolutionRedisUri)
            .WithEnvironment("CACHE_REDIS_DB", "0")
            .WithEnvironment("CACHE_REDIS_PREFIX", "evolution:")
            .WithEnvironment("REDIS_ENABLED", "true")
            .WithEnvironment("REDIS_URI", evolutionRedisUri)

            // Flags de Persistência
            .WithEnvironment("DATABASE_SAVE_DATA_INSTANCE", evolutionSaveInstance)
            .WithEnvironment("DATABASE_SAVE_DATA_NEW_MESSAGE", evolutionSaveNewMessage)
            .WithEnvironment("DATABASE_SAVE_MESSAGE_UPDATE", evolutionSaveMessageUpdate)
            .WithEnvironment("DATABASE_SAVE_DATA_CONTACTS", evolutionSaveContacts)
            .WithEnvironment("DATABASE_SAVE_DATA_CHATS", evolutionSaveChats)
            .WithEnvironment("DATABASE_SAVE_DATA_LABELS", evolutionSaveLabels)
            .WithEnvironment("DATABASE_SAVE_DATA_HISTORIC", evolutionSaveHistoric)

            .WithHttpEndpoint(targetPort: 8080, name: "api")
            .WaitFor(postgres)
            .WaitFor(redis);

        return new EvolutionApiResource(container, evolutionApiKey);
    }
}
