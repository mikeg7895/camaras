using Server.Application.Interfaces;
using Server.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Server.Application.Interfaces.Handlers;
using Server.Application.Handlers;

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
        
        return services;
    }
}