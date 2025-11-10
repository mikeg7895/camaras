using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Application.Strategy.Filters;
public class ResizeFilter : IImageFilter
{
    public string Name => "small";
    public Mat Apply(Mat input)
    {
        var output = new Mat();
        Cv2.Resize(input, output, new OpenCvSharp.Size(input.Width / 2, input.Height / 2));
        return output;
    }
}

