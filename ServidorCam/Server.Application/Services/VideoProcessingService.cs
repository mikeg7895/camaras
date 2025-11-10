using Server.Application.Interfaces;
using Server.Application.Pipeline;
using System.Threading.Channels;

namespace Server.Application.Services;

public class VideoProcessingService(VideoPipeline pipeline) : IVideoProcessingService
{
    private readonly Channel<(string id, string path)> _queue = Channel.CreateUnbounded<(string, string)>();
    private readonly VideoPipeline _pipeline = pipeline;

    public void Enqueue(string id, string path)
        => _queue.Writer.TryWrite((id, path));

    public async Task RunAsync(CancellationToken token)
    {
        Console.WriteLine("VideoProcessingService is running.");
        await foreach (var (id, path) in _queue.Reader.ReadAllAsync(token))
        {
            var ctx = new VideoContext { VideoId = id, VideoPath = path };
            await _pipeline.RunAsync(ctx);
        }
    }
}