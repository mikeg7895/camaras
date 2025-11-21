using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Application.Interfaces;
using Server.Core.Domain.Entities;
using Server.Core.Domain.Interfaces;

namespace Server.Application.Services;

public class VideoService(IVideoRepository repository) : GenericService<Video>(repository), IVideoService
{
    private readonly IVideoRepository _videoRepository = repository;

    public async Task<IEnumerable<Video>> GetByCameraIdAsync(int cameraId)
        => await _videoRepository.GetByCameraIdWithDetailsAsync(cameraId);

    public async Task<(Camera Camera, int VideoCount)> GetCameraWithMostVideosAsync()
        => await _videoRepository.GetCameraWithMostVideosAsync();

    public async Task<IEnumerable<Video>> GetVideosOrderedBySizeAsync()
        => await _videoRepository.GetVideosOrderedBySizeAsync();

    public async Task<long> GetVideoFileSizeAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }
            return 0;
        });
    }
}
