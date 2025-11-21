using System.Collections.Generic;
using System.Threading.Tasks;
using Server.Core.Domain.Entities;

namespace Server.Core.Domain.Interfaces;

public interface IProcessedFrameRepository : IGenericRepository<ProcessedFrame>
{
    Task AddRangeAsync(IEnumerable<ProcessedFrame> processedFrames);
    Task<IEnumerable<ProcessedFrame>> GetByFilterTypeWithDetailsAsync(string filterType);
    Task<IEnumerable<ProcessedFrame>> GetByVideoIdWithDetailsAsync(int videoId);
}
