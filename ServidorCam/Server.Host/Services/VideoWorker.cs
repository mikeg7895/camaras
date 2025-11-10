using Microsoft.Extensions.Hosting;
using Server.Application.Interfaces;

namespace Server.Host.Services;

public class VideoWorker(IVideoProcessingService p) : BackgroundService
{
    private readonly IVideoProcessingService _processor = p;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Video processing worker started.");
        await _processor.RunAsync(stoppingToken);
    }
}
