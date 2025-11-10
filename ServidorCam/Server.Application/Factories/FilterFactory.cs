namespace Server.Application.Factories;

using Server.Application.Strategy.Filters;

public static class FilterFactory
{
    public static List<IImageFilter> DefaultFilters() => new()
    {
        new GrayScaleFilter(),
        new ResizeFilter(),
        new BrightnessFilter(),
        new RotationFilter(45),
        new RotationFilter(90),
        new RotationFilter(180)
    };
}
