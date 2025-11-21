using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Application.Interfaces;
using Server.Core.Domain.Entities;
using Server.Core.Domain.Interfaces;

namespace Server.Application.Services;

public class ProcessedFrameService(IProcessedFrameRepository processedFrameRepository) : GenericService<ProcessedFrame>(processedFrameRepository), IProcessedFrameService
{
    private readonly IProcessedFrameRepository _processedFrameRepository = processedFrameRepository;

    public async Task AddRangeAsync(IEnumerable<ProcessedFrame> processedFrames) 
        => await _processedFrameRepository.AddRangeAsync(processedFrames);

    public async Task<IEnumerable<ProcessedFrame>> GetByFilterTypeAsync(string filterType)
        => await _processedFrameRepository.GetByFilterTypeWithDetailsAsync(filterType);

    public async Task<IEnumerable<ProcessedFrame>> GetByVideoIdAsync(int videoId)
        => await _processedFrameRepository.GetByVideoIdWithDetailsAsync(videoId);
}
