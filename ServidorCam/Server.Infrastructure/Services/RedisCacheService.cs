using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Core.Domain.Interfaces;
using StackExchange.Redis;

namespace Server.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _database = redis.GetDatabase();
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            
            if (value.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
            return null;
        }
    }

    public async Task<byte[]?> GetBytesAsync(string key)
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            
            if (value.IsNullOrEmpty)
                return null;

            return (byte[]?)value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bytes from cache for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            await _database.StringSetAsync(key, json, expiration);
            
            _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
        }
    }

    public async Task SetBytesAsync(string key, byte[] value, TimeSpan? expiration = null)
    {
        try
        {
            await _database.StringSetAsync(key, value, expiration);
            
            _logger.LogDebug("Cached bytes for key: {Key} ({Size} bytes) with expiration: {Expiration}", 
                key, value.Length, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting bytes in cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
            _logger.LogDebug("Removed cache key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing key from cache: {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if key exists in cache: {Key}", key);
            return false;
        }
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
    {
        try
        {
            // Intentar obtener del caché
            var cachedValue = await GetAsync<T>(key);
            if (cachedValue != null)
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return cachedValue;
            }

            // Si no existe, crear y cachear
            _logger.LogDebug("Cache miss for key: {Key}, creating value", key);
            var value = await factory();
            
            if (value != null)
            {
                await SetAsync(key, value, expiration);
            }

            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrCreateAsync for key: {Key}", key);
            // En caso de error, al menos intentar crear el valor
            return await factory();
        }
    }
}
