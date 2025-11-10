using Server.Application.Interfaces;
using Server.Core.Domain.Entities;
using Server.Core.Domain.Interfaces;

namespace Server.Application.Services;

public class ProcessedFrameService(IProcessedFrameRepository processedFrameRepository) : GenericService<ProcessedFrame>(processedFrameRepository), IProcessedFrameService
{
    public async Task AddRangeAsync(IEnumerable<ProcessedFrame> processedFrames) 
        => await ((IProcessedFrameRepository)_repository).AddRangeAsync(processedFrames);
}
