using OpenCvSharp;

namespace Server.Application.Pipeline;

public class ExtractFrameStep : IVideoStep
{
    public async Task ExecuteAsync(VideoContext context)
    {
        Console.WriteLine($"Extracting frames for video: {context.VideoId}");
        string outputDir = Path.Combine("frames", context.VideoId);
        Directory.CreateDirectory(outputDir);

        using var cap = new VideoCapture(context.VideoPath);
        var mat = new Mat();
        int frameNum = 0;

        while (cap.Read(mat))
        {
            string framePath = Path.Combine(outputDir, $"frame_{frameNum:D4}.jpg");
            Cv2.ImWrite(framePath, mat);
            context.Frames.Add(framePath);
            frameNum++;
        }

        await Task.CompletedTask;
    }
}
