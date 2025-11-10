using Server.Application.Strategy.Filters;

namespace Server.Application.Pipeline;

public class ApplyFilterStep(FilterProcessor processor) : IVideoStep
{
    private readonly FilterProcessor _processor = processor;

    public async Task ExecuteAsync(VideoContext context)
    {
        Console.WriteLine($"Applying filters to frames for video: {context.VideoId}");
        foreach (var framePath in context.Frames)
        {
            using var img = OpenCvSharp.Cv2.ImRead(framePath);
            await _processor.ApplyAll(img, Path.Combine(Path.GetDirectoryName(framePath)!, "filters", Path.GetFileNameWithoutExtension(framePath)), int.Parse(context.VideoId));
        }
        await Task.CompletedTask;
    }
}
