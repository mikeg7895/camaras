using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Server.Core.Domain.Entities;
using Server.Core.Domain.Interfaces;
using Server.Infrastructure.Persistence;

namespace Server.Infrastructure.Repositories;

public class CameraRepository(ServerDbContext context) : GenericRepository<Camera>(context), ICameraRepository
{
    public override IQueryable<Camera> GetAll() => Entities.Include(c => c.User).Include(c => c.Videos);

    public override async Task<IEnumerable<Camera>> GetAllAsync()
    {
        return await Entities
            .Include(c => c.User)
            .Include(c => c.Videos)
            .ToListAsync();
    }

    public async Task<IEnumerable<Camera>> GetByUserIdAsync(int userId)
    {
        return await Entities
            .Where(c => c.UserId == userId)
            .Include(c => c.User)
            .Include(c => c.Videos)
            .ToListAsync();
    }

    public async Task<Camera?> GetByDeviceIdAsync(Guid deviceId)
    {
        return await Entities
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.DeviceId == deviceId);
    }

    public async Task<Camera?> GetCameraWithDetailsAsync(int cameraId)
    {
        return await Entities
            .Include(c => c.User)
            .Include(c => c.Videos)
                .ThenInclude(v => v.ProcessedFrames)
            .FirstOrDefaultAsync(c => c.Id == cameraId);
    }
}
