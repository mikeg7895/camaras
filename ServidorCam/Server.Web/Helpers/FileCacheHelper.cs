using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Core.Domain.Interfaces;

namespace Server.Web.Helpers;

public class FileCacheHelper
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<FileCacheHelper> _logger;
    
    // Configuración
    private readonly TimeSpan _videoThumbnailCacheExpiration = TimeSpan.FromHours(24);
    private readonly TimeSpan _frameCacheExpiration = TimeSpan.FromHours(12);
    private const long MaxCacheFileSize = 10 * 1024 * 1024; // 10 MB máximo para cachear

    public FileCacheHelper(
        ICacheService cacheService,
        ILogger<FileCacheHelper> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<byte[]?> GetOrCacheFileAsync(string filePath, string cacheKey, TimeSpan? expiration = null)
    {
        try
        {
            // Verificar si el archivo existe
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("File not found: {FilePath}", filePath);
                return null;
            }

            var fileInfo = new FileInfo(filePath);
            
            // Solo cachear archivos pequeños
            if (fileInfo.Length > MaxCacheFileSize)
            {
                _logger.LogDebug("File too large to cache: {FilePath} ({Size} bytes)", filePath, fileInfo.Length);
                return await File.ReadAllBytesAsync(filePath);
            }

            // Intentar obtener del caché
            var cachedFile = await _cacheService.GetBytesAsync(cacheKey);
            if (cachedFile != null)
            {
                _logger.LogDebug("Cache hit for file: {CacheKey}", cacheKey);
                return cachedFile;
            }

            // Si no está en caché, leer del disco y cachear
            _logger.LogDebug("Cache miss for file: {CacheKey}, loading from disk", cacheKey);
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            
            await _cacheService.SetBytesAsync(cacheKey, fileBytes, expiration);
            
            return fileBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting or caching file: {FilePath}", filePath);
            
            // En caso de error, intentar leer directamente del disco
            if (File.Exists(filePath))
            {
                return await File.ReadAllBytesAsync(filePath);
            }
            
            return null;
        }
    }

    public async Task<byte[]?> GetOrCacheFrameAsync(int frameId, string filePath)
    {
        var cacheKey = $"frame:{frameId}";
        return await GetOrCacheFileAsync(filePath, cacheKey, _frameCacheExpiration);
    }

    public async Task InvalidateFileCacheAsync(string cacheKey)
    {
        try
        {
            await _cacheService.RemoveAsync(cacheKey);
            _logger.LogDebug("Invalidated file cache: {CacheKey}", cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating file cache: {CacheKey}", cacheKey);
        }
    }

    public async Task InvalidateFrameCacheAsync(int frameId)
    {
        await InvalidateFileCacheAsync($"frame:{frameId}");
    }
}
