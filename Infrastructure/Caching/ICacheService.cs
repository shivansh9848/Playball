namespace Assignment_Example_HU.Infrastructure.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task<long> IncrementAsync(string key, long value = 1);
    Task<long> DecrementAsync(string key, long value = 1);
    Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan? expiry = null);
}
