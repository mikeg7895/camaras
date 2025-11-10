namespace Server.Application.Interfaces;

public interface IVideoProcessingService
{
    void Enqueue(string id, string path);
    Task RunAsync(CancellationToken cancellationToken);
}