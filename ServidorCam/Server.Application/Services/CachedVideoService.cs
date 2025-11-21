using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Application.Interfaces;
using Server.Core.Domain.Entities;
using Server.Core.Domain.Interfaces;

namespace Server.Application.Services;

public class CachedVideoService : IVideoService
{
    private readonly IVideoService _videoService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachedVideoService> _logger;
    
    // Configuración de expiración del caché
    private readonly TimeSpan _videoListCacheExpiration = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _videoDetailsCacheExpiration = TimeSpan.FromMinutes(10);
    private readonly TimeSpan _videoFileCacheExpiration = TimeSpan.FromHours(1);

    public CachedVideoService(
        IVideoService videoService,
        ICacheService cacheService,
        ILogger<CachedVideoService> logger)
    {
        _videoService = videoService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Video?> GetByIdAsync(int id)
    {
        var cacheKey = $"video:{id}";
        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            () => _videoService.GetByIdAsync(id),
            _videoDetailsCacheExpiration
        );
    }

    public async Task<IEnumerable<Video>> GetAllAsync()
    {
        var cacheKey = "videos:all";
        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            () => _videoService.GetAllAsync(),
            _videoListCacheExpiration
        );
    }

    public async Task<IEnumerable<Video>> GetByCameraIdAsync(int cameraId)
    {
        var cacheKey = $"videos:camera:{cameraId}";
        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            () => _videoService.GetByCameraIdAsync(cameraId),
            _videoListCacheExpiration
        );
    }

    public async Task<(Camera Camera, int VideoCount)> GetCameraWithMostVideosAsync()
    {
        // Este endpoint no se cachea porque necesita ser preciso
        return await _videoService.GetCameraWithMostVideosAsync();
    }

    public async Task<IEnumerable<Video>> GetVideosOrderedBySizeAsync()
    {
        var cacheKey = "videos:ordered:size";
        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            () => _videoService.GetVideosOrderedBySizeAsync(),
            _videoListCacheExpiration
        );
    }

    public async Task<long> GetVideoFileSizeAsync(string filePath)
    {
        return await _videoService.GetVideoFileSizeAsync(filePath);
    }

    public async Task<Video> AddAsync(Video entity)
    {
        var result = await _videoService.AddAsync(entity);
        
        await InvalidateVideoCaches(entity.CameraId);
        
        return result;
    }

    public void Update(Video entity)
    {
        _videoService.Update(entity);
        
        Task.Run(() => InvalidateVideoCaches(entity.CameraId, entity.Id)).Wait();
    }

    public async Task<bool> Delete(int id)
    {
        var video = await GetByIdAsync(id);
        var result = await _videoService.Delete(id);
        
        // Invalidar cachés relacionados
        if (video != null && result)
        {
            await InvalidateVideoCaches(video.CameraId, id);
        }
        
        return result;
    }

    public async Task SaveChangesAsync()
    {
        await _videoService.SaveChangesAsync();
    }

    private async Task InvalidateVideoCaches(int cameraId, int? videoId = null)
    {
        try
        {
            // Invalidar caché de video específico si se proporciona
            if (videoId.HasValue)
            {
                await _cacheService.RemoveAsync($"video:{videoId}");
                await _cacheService.RemoveAsync($"videofile:{videoId}");
            }
            
            // Invalidar caché de videos por cámara
            await _cacheService.RemoveAsync($"videos:camera:{cameraId}");
            
            // Invalidar caché de todos los videos
            await _cacheService.RemoveAsync("videos:all");
            await _cacheService.RemoveAsync("videos:ordered:size");
            
            _logger.LogDebug("Invalidated video caches for camera {CameraId}", cameraId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating video caches");
        }
    }
}
