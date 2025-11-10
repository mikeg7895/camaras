using OpenCvSharp;

namespace Server.Application.Strategy.Filters;

public class BrightnessFilter : IImageFilter
{
    public string Name => "bright";
    public Mat Apply(Mat input)
    {
        var output = new Mat();
        input.ConvertTo(output, -1, 1, 50);
        return output;
    }
}
