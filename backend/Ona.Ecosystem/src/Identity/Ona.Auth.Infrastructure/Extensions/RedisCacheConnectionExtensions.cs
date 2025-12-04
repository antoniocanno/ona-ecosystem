using Microsoft.Extensions.Hosting;

namespace Ona.Auth.Infrastructure.Extensions
{
    public static class RedisCacheConnectionExtensions
    {
        public static TBuilder AddRedisDistributedCache<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            builder.AddRedisDistributedCache("cache");

            return builder;
        }
    }
}
