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
}