using Microsoft.Extensions.Caching.Distributed;
using Ona.Auth.Application.Interfaces.Services;
using StackExchange.Redis;
using System.Text.Json;

namespace Ona.Auth.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly IDatabase _redisDb;

        public RedisCacheService(
            IDistributedCache cache,
            IConnectionMultiplexer muxer)
        {
            _cache = cache;
            _redisDb = muxer.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var data = await _cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(data))
                return default;

            return JsonSerializer.Deserialize<T>(data);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            var jsonData = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, jsonData, options);
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

        public async Task<long> IncrementAsync(string key, TimeSpan expiration)
        {
            long newValue = await _redisDb.StringIncrementAsync(key);

            if (newValue == 1)
                await _redisDb.KeyExpireAsync(key, expiration);

            return newValue;
        }
    }
}
