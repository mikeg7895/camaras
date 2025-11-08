using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Server.Core.Domain.Interfaces;
using Server.Infrastructure.Communication.Tcp;
using Server.Infrastructure.Persistence;
using Server.Infrastructure.Repositories;

namespace Server.Infrastructure;

public static class RegisterObjects
{
    public static IServiceCollection AddInfrastructureObjects(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ServerDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        // Repositorios concretos
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICameraRepository, CameraRepository>();
        services.AddScoped<IVideoRepository, VideoRepository>();
        services.AddScoped<IProcessedFrameRepository, ProcessedFrameRepository>();

        // TCP Server
        services.AddSingleton<TcpServer>();

        return services;
    }
}
