using OpenCvSharp;
using Server.Application.Interfaces;
using Server.Core.Domain.Entities;

namespace Server.Application.Strategy.Filters;

public class FilterProcessor(IEnumerable<IImageFilter> filters, IProcessedFrameService processedFrameService)
{
    private readonly IEnumerable<IImageFilter> _filters = filters;
    private readonly IProcessedFrameService _processedFrameService = processedFrameService;

    public async Task ApplyAll(Mat image, string outputBase, int videoId)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputBase)!);
        var processedFrames = new List<ProcessedFrame>();

        foreach (var filter in _filters)
        {
            var result = filter.Apply(image);
            var path = $"{outputBase}_{filter.Name}.jpg";
            Cv2.ImWrite(path, result);
            processedFrames.Add(new ProcessedFrame
            {
                FilePath = path,
                FilterType = filter.Name,
                ProcessedAt = DateTime.Now,
                VideoId = videoId
            });
        }
        await _processedFrameService.AddRangeAsync(processedFrames);
        await _processedFrameService.SaveChangesAsync();
    }
}
