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
        var evolutionApiKey = builder.AddParameter("Evolution-ApiKey", secret: true);
        var evolutionApiUrl = builder.AddParameter("Evolution-ApiUrl");
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
        var evolutionCacheRedisEnabled = builder.AddParameter("Evolution-CacheRedisEnabled");
        var evolutionCacheRedisPrefixKey = builder.AddParameter("Evolution-CacheRedisPrefixKey");
        var evolutionCacheRedisSaveInstances = builder.AddParameter("Evolution-CacheRedisSaveInstances");
        var evolutionCacheLocalEnabled = builder.AddParameter("Evolution-CacheLocalEnabled");

        // --- URIs de Conexão ---
        var evolutionPostgresUri = ReferenceExpression.Create(
            $"postgresql://postgres:{pgPassword}@postgres:5432/evolution-db?schema=public");

        // --- Container da Evolution API ---
        var container = builder.AddContainer("evolution-api", "atendai/evolution-api")
            .WithEnvironment("AUTHENTICATION_TYPE", "apikey")
            .WithEnvironment("AUTHENTICATION_API_KEY", evolutionApiKey)
            .WithEnvironment("SERVER_URL", evolutionApiUrl)
            .WithEnvironment("TZ", evolutionTimeZone)

            // Configuração de Banco de Dados (Postgres)
            .WithEnvironment("DATABASE_ENABLED", evolutionDbEnabled)
            .WithEnvironment("DATABASE_PROVIDER", evolutionDbProvider)
            .WithEnvironment("DATABASE_CONNECTION_URI", evolutionPostgresUri)
            .WithEnvironment("DATABASE_CONNECTION_CLIENT_NAME", evolutionDbClientName)

            // Configuração de Cache (Redis)
            .WithEnvironment("CACHE_REDIS_ENABLED", evolutionCacheRedisEnabled)
            .WithEnvironment("CACHE_REDIS_URI", ReferenceExpression.Create($"redis://{redis.GetEndpoint("tcp")}/0"))
            .WithEnvironment("CACHE_REDIS_PREFIX_KEY", evolutionCacheRedisPrefixKey)
            .WithEnvironment("CACHE_REDIS_SAVE_INSTANCES", evolutionCacheRedisSaveInstances)
            .WithEnvironment("REDIS_ENABLED", evolutionCacheRedisEnabled)
            .WithEnvironment("REDIS_URI", ReferenceExpression.Create($"redis://{redis.GetEndpoint("tcp")}/0"))
            .WithEnvironment("CACHE_LOCAL_ENABLED", evolutionCacheLocalEnabled)

            // Flags de Persistência
            .WithEnvironment("DATABASE_SAVE_DATA_INSTANCE", evolutionSaveInstance)
            .WithEnvironment("DATABASE_SAVE_DATA_NEW_MESSAGE", evolutionSaveNewMessage)
            .WithEnvironment("DATABASE_SAVE_MESSAGE_UPDATE", evolutionSaveMessageUpdate)
            .WithEnvironment("DATABASE_SAVE_DATA_CONTACTS", evolutionSaveContacts)
            .WithEnvironment("DATABASE_SAVE_DATA_CHATS", evolutionSaveChats)
            .WithEnvironment("DATABASE_SAVE_DATA_LABELS", evolutionSaveLabels)
            .WithEnvironment("DATABASE_SAVE_DATA_HISTORIC", evolutionSaveHistoric)

            .WithEnvironment("CONFIG_SESSION_PHONE_VERSION", "2.3000.1031952138")
            .WithEnvironment("CONFIG_SESSION_PHONE_CLIENT", "Commit")
            .WithEnvironment("CONFIG_SESSION_PHONE_NAME", "Chrome")

            .WithHttpEndpoint(targetPort: 8080, name: "api")
            .WaitFor(postgres)
            .WaitFor(redis);

        return new EvolutionApiResource(container, evolutionApiKey);
    }
}
