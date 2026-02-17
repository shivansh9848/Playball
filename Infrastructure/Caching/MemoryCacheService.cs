using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Assignment_Example_HU.Infrastructure.Caching;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;

    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        try
        {
            if (_cache.TryGetValue(key, out T? value))
            {
                return Task.FromResult(value);
            }
            return Task.FromResult<T?>(default);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
            return Task.FromResult<T?>(default);
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var cacheOptions = new MemoryCacheEntryOptions();
            if (expiry.HasValue)
            {
                cacheOptions.AbsoluteExpirationRelativeToNow = expiry.Value;
            }
            else
            {
                cacheOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            }

            _cache.Set(key, value, cacheOptions);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
            return Task.CompletedTask;
        }
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from cache for key: {Key}", key);
            return Task.CompletedTask;
        }
    }

    public Task<bool> ExistsAsync(string key)
    {
        return Task.FromResult(_cache.TryGetValue(key, out _));
    }

    public async Task<long> IncrementAsync(string key, long value = 1)
    {
        try
        {
            var current = await GetAsync<long>(key);
            var newValue = current + value;
            await SetAsync(key, newValue, TimeSpan.FromMinutes(10));
            return newValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing value in cache for key: {Key}", key);
            return 0;
        }
    }

    public async Task<long> DecrementAsync(string key, long value = 1)
    {
        try
        {
            var current = await GetAsync<long>(key);
            var newValue = Math.Max(0, current - value);
            await SetAsync(key, newValue, TimeSpan.FromMinutes(10));
            return newValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrementing value in cache for key: {Key}", key);
            return 0;
        }
    }

    public async Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            if (!await ExistsAsync(key))
            {
                await SetAsync(key, value, expiry);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SetIfNotExists for key: {Key}", key);
            return false;
        }
    }
}
