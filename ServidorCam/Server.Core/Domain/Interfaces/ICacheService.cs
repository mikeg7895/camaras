namespace Server.Core.Domain.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task<byte[]?> GetBytesAsync(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task SetBytesAsync(string key, byte[] value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class;
}
