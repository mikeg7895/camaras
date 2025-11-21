using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Application.Interfaces;

namespace Server.Web.Services;

public class ConnectionSyncService : BackgroundService
{
    private readonly IRealtimeConnectionService? _realtimeService;
    private readonly IConnectionTrackingService _connectionTracker;
    private readonly ILogger<ConnectionSyncService> _logger;

    public ConnectionSyncService(
        IConnectionTrackingService connectionTracker,
        ILogger<ConnectionSyncService> logger,
        IRealtimeConnectionService? realtimeService = null)
    {
        _realtimeService = realtimeService;
        _connectionTracker = connectionTracker;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_realtimeService == null)
        {
            _logger.LogWarning("Connection Sync Service disabled - Redis not configured");
            return;
        }

        _logger.LogInformation("Connection Sync Service started - listening to Redis events");

        try
        {
            await _realtimeService.SubscribeToConnectionEventsAsync(async (username, ipAddress, isConnected) =>
            {
                try
                {
                    if (isConnected)
                    {
                        _connectionTracker.RegisterConnection(username, ipAddress);
                        _logger.LogInformation("Synced connection: {Username} from {IpAddress}", username, ipAddress);
                    }
                    else
                    {
                        _connectionTracker.UnregisterConnection(username, ipAddress);
                        _logger.LogInformation("Synced disconnection: {Username} from {IpAddress}", username, ipAddress);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing connection event");
                }
                
                await Task.CompletedTask;
            });

            // Mantener el servicio ejecutándose
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection Sync Service failed");
        }
    }
}
