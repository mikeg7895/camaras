using OpenCvSharp;

namespace Server.Application.Strategy.Filters;
public class GrayScaleFilter : IImageFilter
{
    public string Name => "gray";
    public Mat Apply(Mat input)
    {
        var output = new Mat();
        Cv2.CvtColor(input, output, ColorConversionCodes.BGR2GRAY);
        return output;
    }
}

