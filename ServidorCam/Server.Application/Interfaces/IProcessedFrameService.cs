using Server.Core.Domain.Entities;

namespace Server.Application.Interfaces;

public interface IProcessedFrameService : IGenericService<ProcessedFrame>
{
    Task AddRangeAsync(IEnumerable<ProcessedFrame> processedFrames);
}
