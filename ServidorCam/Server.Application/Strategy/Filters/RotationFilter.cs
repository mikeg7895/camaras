using OpenCvSharp;

namespace Server.Application.Strategy.Filters;

public class RotationFilter : IImageFilter
{
    private readonly double _angle;
    public RotationFilter(double angle) => _angle = angle;
    public string Name => $"rot{_angle}";
    public Mat Apply(Mat input)
    {
        var center = new OpenCvSharp.Point2f(input.Width / 2, input.Height / 2);
        var m = OpenCvSharp.Cv2.GetRotationMatrix2D(center, _angle, 1.0);
        var output = new OpenCvSharp.Mat();
        OpenCvSharp.Cv2.WarpAffine(input, output, m, input.Size());
        return output;
    }
}
