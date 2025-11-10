using Server.Application.Interfaces;
using Server.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Server.Application.Interfaces.Handlers;
using Server.Application.Handlers;
using Server.Application.Strategy.Filters;
using Server.Application.Factories;
using Server.Application.Pipeline;

namespace Server.Application;

public static class RegisterObjects
{
    public static IServiceCollection AddApplicationObjects(this IServiceCollection services)
    {
        services.AddScoped<ITcpRequestHandler, TcpRequestDispatcher>();

        services.Scan(scan => scan
            .FromAssemblyOf<LoginCommandHandler>()
            .AddClasses(classes => classes.AssignableTo<ITcpCommandHandler>())
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICameraService, CameraService>();
        services.AddScoped<IVideoService, VideoService>();
        services.AddSingleton<IVideoProcessingService, VideoProcessingService>();
        services.AddScoped<IProcessedFrameService, ProcessedFrameService>();

        // Registrar filtros usando la factory
        services.AddTransient<IEnumerable<IImageFilter>>(provider => FilterFactory.DefaultFilters());
        services.AddTransient<FilterProcessor>();

        // Registrar los pasos del pipeline EN ORDEN ESPECÍFICO
        services.AddTransient<IEnumerable<IVideoStep>>(provider => new List<IVideoStep>
        {
            new ExtractFrameStep(),
            new ApplyFilterStep(provider.GetRequiredService<FilterProcessor>()),
            new StoreResult(provider.GetRequiredService<IVideoService>())
        });

        // Registrar el VideoPipeline
        services.AddTransient<VideoPipeline>();
        
        return services;
    }
}