using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Ona.Commit.Infrastructure.Gateways.Evolution
{
    /// <summary>
    /// Gerenciador de fila e rate limiting distribuído para o WhatsApp.
    /// Utiliza Redis (IDistributedCache) para coordenar o tempo de envio entre múltiplas instâncias
    /// e um SemaphoreSlim local para garantir a ordem dentro da mesma aplicação.
    /// </summary>
    public class EvolutionMessageDispatcher
    {
        private readonly IDistributedCache _cache;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EvolutionMessageDispatcher> _logger;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _instanceLocks = new();

        public EvolutionMessageDispatcher(
            IDistributedCache cache,
            IServiceProvider serviceProvider,
            ILogger<EvolutionMessageDispatcher> logger)
        {
            _cache = cache;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<string> EnqueueAsync(string instanceName, Func<EvolutionWhatsAppGateway, Task<string>> action)
        {
            var semaphore = _instanceLocks.GetOrAdd(instanceName, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();

            try
            {
                var delay = await CalculateDelayAsync(instanceName);

                if (delay > TimeSpan.Zero)
                {
                    _logger.LogInformation("[{Instance}] Humanização Distribuída: Aguardando {Delay}s antes do envio...", instanceName, delay.TotalSeconds);
                    await Task.Delay(delay);
                }

                using var scope = _serviceProvider.CreateScope();
                var client = scope.ServiceProvider.GetRequiredService<EvolutionWhatsAppGateway>();
                return await action(client);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task<TimeSpan> CalculateDelayAsync(string instanceName)
        {
            var key = $"whatsapp:last_sent:{instanceName}";
            var machineTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var random = new Random();
            var jitterSeconds = random.Next(30, 51);
            var minIntervalMs = jitterSeconds * 1000;

            long lastSentTime = 0;
            var cachedValue = await _cache.GetAsync(key);
            if (cachedValue != null)
            {
                lastSentTime = BitConverter.ToInt64(cachedValue);
            }

            var nextAllowedTime = Math.Max(machineTime, lastSentTime + minIntervalMs);

            await _cache.SetAsync(key, BitConverter.GetBytes(nextAllowedTime), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });

            var waitMs = nextAllowedTime - machineTime;
            return waitMs > 0 ? TimeSpan.FromMilliseconds(waitMs) : TimeSpan.Zero;
        }
    }
}
