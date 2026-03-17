namespace Ona.AppHost;

public static class EvolutionApiExtensions
{
    public record EvolutionApiResource(IResourceBuilder<ContainerResource> Container, IResourceBuilder<ParameterResource> ApiKey);

    public static EvolutionApiResource AddEvolutionApi(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<IResourceWithConnectionString> postgres,
        IResourceBuilder<ParameterResource> pgPassword,
        IResourceBuilder<RedisResource> redis,
        IResourceBuilder<RabbitMQServerResource> rabbitMq)
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

        // RabbitMQ Parâmetros
        var rabbitMqEnabled = builder.AddParameter("RabbitMq-Enabled");
        var rabbitMqGlobalEnabled = builder.AddParameter("RabbitMq-GlobalEnabled");
        var rabbitMqExchange = builder.AddParameter("RabbitMq-ExchangeName");
        var rabbitMqEventsConnectionUpdate = builder.AddParameter("RabbitMq-Events-ConnectionUpdate");
        var rabbitMqEventsMessagesUpsert = builder.AddParameter("RabbitMq-Events-MessagesUpsert");

        // --- URIs de Conexão ---
        var evolutionPostgresUri = ReferenceExpression.Create(
            $"postgresql://postgres:{pgPassword}@postgres:5432/evolution-db?schema=public");

        // --- Container da Evolution API ---
        var container = builder.AddContainer("evolution-api", "atendai/evolution-api:v2.2.3")
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
            .WithEnvironment("CACHE_REDIS_URI", ReferenceExpression.Create($"redis://{redis.Resource.Name}:6379"))
            .WithEnvironment("CACHE_REDIS_PREFIX_KEY", evolutionCacheRedisPrefixKey)
            .WithEnvironment("CACHE_REDIS_SAVE_INSTANCES", evolutionCacheRedisSaveInstances)
            .WithEnvironment("REDIS_ENABLED", evolutionCacheRedisEnabled)
            .WithEnvironment("REDIS_URI", ReferenceExpression.Create($"redis://{redis.Resource.Name}:6379"))
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
            .WithEnvironment("CONFIG_SESSION_PHONE_CLIENT", "Ona System")
            .WithEnvironment("CONFIG_SESSION_PHONE_NAME", "Chrome")

            // Configuração RabbitMQ
            .WithEnvironment("RABBITMQ_ENABLED", rabbitMqEnabled)
            .WithEnvironment("RABBITMQ_URI", ReferenceExpression.Create($"amqp://{rabbitMq.Resource.Name}:5672"))
            .WithEnvironment("RABBITMQ_EXCHANGE_NAME", rabbitMqExchange)
            .WithEnvironment("RABBITMQ_GLOBAL_ENABLED", rabbitMqGlobalEnabled)
            .WithEnvironment("RABBITMQ_EVENTS_CONNECTION_UPDATE", rabbitMqEventsConnectionUpdate)
            .WithEnvironment("RABBITMQ_EVENTS_MESSAGES_UPSERT", rabbitMqEventsMessagesUpsert)

            .WithHttpEndpoint(targetPort: 8080, name: "api")
            .WithReference(rabbitMq)
            .WaitFor(postgres)
            .WaitFor(redis)
            .WaitFor(rabbitMq);

        return new EvolutionApiResource(container, evolutionApiKey);
    }
}
