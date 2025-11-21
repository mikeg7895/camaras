using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Core.Domain.Entities;

namespace Server.Core.Domain.Interfaces;

public interface IVideoRepository : IGenericRepository<Video>
{
    Task<IEnumerable<Video>> GetByCameraIdWithDetailsAsync(int cameraId);
    Task<(Camera Camera, int VideoCount)> GetCameraWithMostVideosAsync();
    Task<IEnumerable<Video>> GetVideosOrderedBySizeAsync();
}
