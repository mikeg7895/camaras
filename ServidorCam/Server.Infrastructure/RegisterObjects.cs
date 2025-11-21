using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Server.Application.Interfaces;
using Server.Core.Domain.Interfaces;
using Server.Infrastructure.Communication.Tcp;
using Server.Infrastructure.Persistence;
using Server.Infrastructure.Repositories;
using Server.Infrastructure.Services;
using StackExchange.Redis;

namespace Server.Infrastructure;

public static class RegisterObjects
{
    public static IServiceCollection AddInfrastructureObjects(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ServerDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        // Redis
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configuration = ConfigurationOptions.Parse(redisConnectionString);
                return ConnectionMultiplexer.Connect(configuration);
            });
            services.AddSingleton<ICacheService, RedisCacheService>();
            services.AddSingleton<IRealtimeConnectionService, RealtimeConnectionService>();
        }

        // Repositorios concretos
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICameraRepository, CameraRepository>();
        services.AddScoped<IVideoRepository, VideoRepository>();
        services.AddScoped<IProcessedFrameRepository, ProcessedFrameRepository>();

        // TCP Server
        services.AddSingleton<TcpServer>();
        
        // Connection Tracking Service (Singleton para compartir estado en memoria)
        services.AddSingleton<IConnectionTrackingService, ConnectionTrackingService>();

        return services;
    }
}
