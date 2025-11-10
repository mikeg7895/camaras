using OpenCvSharp;

namespace Server.Application.Strategy.Filters;

public interface IImageFilter
{
    string Name { get; }
    Mat Apply(Mat input);
}
