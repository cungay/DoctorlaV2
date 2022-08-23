using Doctorla.Application.Common.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Doctorla.Infrastructure.Caching;

public class LocalCacheService : ICacheService
{
    private readonly ILogger<LocalCacheService> logger = null;
    private readonly IMemoryCache cache = null;

    public LocalCacheService(IMemoryCache cache, ILogger<LocalCacheService> logger) =>
        (this.cache, this.logger) = (cache, logger);

    public T? Get<T>(string key) =>
        cache.Get<T>(key);

    public Task<T?> GetAsync<T>(string key, CancellationToken token = default) =>
        Task.FromResult(Get<T>(key));

    public void Refresh(string key) =>
        cache.TryGetValue(key, out _);

    public Task RefreshAsync(string key, CancellationToken token = default)
    {
        Refresh(key);
        return Task.CompletedTask;
    }

    public void Remove(string key) =>
        cache.Remove(key);

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        Remove(key);
        return Task.CompletedTask;
    }

    public void Set<T>(string key, T value, TimeSpan? slidingExpiration = null)
    {
        slidingExpiration ??= TimeSpan.FromMinutes(10);
        cache.Set(key, value, new MemoryCacheEntryOptions { SlidingExpiration = slidingExpiration });
        logger.LogDebug($"Added to Cache : {key}", key);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, CancellationToken token = default)
    {
        Set(key, value, slidingExpiration);
        return Task.CompletedTask;
    }
}