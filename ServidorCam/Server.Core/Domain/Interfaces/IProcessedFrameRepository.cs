using Server.Core.Domain.Entities;

namespace Server.Core.Domain.Interfaces;

public interface IProcessedFrameRepository : IGenericRepository<ProcessedFrame>
{
    Task AddRangeAsync(IEnumerable<ProcessedFrame> processedFrames);
}