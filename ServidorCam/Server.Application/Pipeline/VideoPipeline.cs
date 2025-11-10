namespace Server.Application.Pipeline;

public class VideoPipeline(IEnumerable<IVideoStep> steps)
{
    private readonly IEnumerable<IVideoStep> _steps = steps;

    public async Task RunAsync(VideoContext context)
    {
        Console.WriteLine("Starting video pipeline...");
        Console.WriteLine($"Steps to execute: {_steps.Count()}");
        foreach (var step in _steps)
        {
            Console.WriteLine($"Executing step: {step.GetType().Name}");
            await step.ExecuteAsync(context);
        }
    }
}
