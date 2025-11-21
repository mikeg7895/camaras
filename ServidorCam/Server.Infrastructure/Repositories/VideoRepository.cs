using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Server.Core.Domain.Entities;
using Server.Core.Domain.Interfaces;
using Server.Infrastructure.Persistence;

namespace Server.Infrastructure.Repositories;

public class VideoRepository(ServerDbContext context) : GenericRepository<Video>(context), IVideoRepository
{
    public async Task<IEnumerable<Video>> GetByCameraIdWithDetailsAsync(int cameraId)
    {
        return await Entities
            .Where(v => v.CameraId == cameraId)
            .Include(v => v.Camera)
                .ThenInclude(c => c.User)
            .Include(v => v.ProcessedFrames)
            .OrderByDescending(v => v.RecordedAt)
            .ToListAsync();
    }

    public async Task<(Camera Camera, int VideoCount)> GetCameraWithMostVideosAsync()
    {
        var cameraStats = await Entities
            .GroupBy(v => v.CameraId)
            .Select(g => new { CameraId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .FirstOrDefaultAsync();

        if (cameraStats == null)
        {
            return (null!, 0);
        }

        var camera = await _context.Cameras
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == cameraStats.CameraId);

        return (camera!, cameraStats.Count);
    }

    public async Task<IEnumerable<Video>> GetVideosOrderedBySizeAsync()
    {
        return await Entities
            .Include(v => v.Camera)
                .ThenInclude(c => c.User)
            .Include(v => v.ProcessedFrames)
            .OrderByDescending(v => v.RecordedAt)
            .ToListAsync();
    }
}
