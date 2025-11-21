using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Server.Core.Domain.Entities;
using Server.Core.Domain.Interfaces;
using Server.Infrastructure.Persistence;

namespace Server.Infrastructure.Repositories;

public class ProcessedFrameRepository(ServerDbContext context) : GenericRepository<ProcessedFrame>(context), IProcessedFrameRepository
{
    public async Task AddRangeAsync(IEnumerable<ProcessedFrame> valores)
    {
        await Entities.AddRangeAsync(valores);
    }

    public override IQueryable<ProcessedFrame> GetAll() => Entities.Include(pf => pf.Video).ThenInclude(v => v.Camera).ThenInclude(c => c.User);

    public override async Task<IEnumerable<ProcessedFrame>> GetAllAsync()
    {
        return await Entities
            .Include(pf => pf.Video)
                .ThenInclude(v => v.Camera)
                    .ThenInclude(c => c.User)
            .OrderByDescending(pf => pf.ProcessedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProcessedFrame>> GetByFilterTypeWithDetailsAsync(string filterType)
    {
        return await Entities
            .Where(pf => pf.FilterType == filterType)
            .Include(pf => pf.Video)
                .ThenInclude(v => v.Camera)
                    .ThenInclude(c => c.User)
            .OrderByDescending(pf => pf.ProcessedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProcessedFrame>> GetByVideoIdWithDetailsAsync(int videoId)
    {
        return await Entities
            .Where(pf => pf.VideoId == videoId)
            .Include(pf => pf.Video)
                .ThenInclude(v => v.Camera)
                    .ThenInclude(c => c.User)
            .OrderByDescending(pf => pf.ProcessedAt)
            .ToListAsync();
    }
}
