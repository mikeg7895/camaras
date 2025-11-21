using Server.Core.Domain.Entities;

namespace Server.Application.Interfaces;

public interface IVideoService : IGenericService<Video>
{
    Task<IEnumerable<Video>> GetByCameraIdAsync(int cameraId);
    Task<(Camera Camera, int VideoCount)> GetCameraWithMostVideosAsync();
    Task<IEnumerable<Video>> GetVideosOrderedBySizeAsync();
    Task<long> GetVideoFileSizeAsync(string filePath);
}