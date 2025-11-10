using Server.Application.Interfaces;

namespace Server.Application.Pipeline;

public class StoreResult(IVideoService videoService) : IVideoStep
{
    private readonly IVideoService _videoService = videoService;

    public async Task ExecuteAsync(VideoContext videoContext)
    {
        Console.WriteLine($"Storing result for video: {videoContext.VideoId}");
        var video = await _videoService.GetByIdAsync(int.Parse(videoContext.VideoId));
        video!.FrameCount = videoContext.Frames.Count;
        _videoService.Update(video);
        await _videoService.SaveChangesAsync();
    }
}
