namespace Server.Application.Pipeline;

public interface IVideoStep
{
    Task ExecuteAsync(VideoContext videoContext);
}
