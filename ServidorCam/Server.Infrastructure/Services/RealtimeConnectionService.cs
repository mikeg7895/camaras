using System.Text.Json;
using Microsoft.Extensions.Logging;
using Server.Application.Interfaces;
using StackExchange.Redis;

namespace Server.Infrastructure.Services;

public class RealtimeConnectionService : IRealtimeConnectionService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RealtimeConnectionService> _logger;
    private const string ConnectionChannel = "tcp-connections";

    public RealtimeConnectionService(
        IConnectionMultiplexer redis,
        ILogger<RealtimeConnectionService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task PublishConnectionAsync(string username, string ipAddress, bool isConnected)
    {
        try
        {
            var connectionEvent = new ConnectionEvent
            {
                Username = username,
                IpAddress = ipAddress,
                IsConnected = isConnected,
                Timestamp = DateTime.UtcNow
            };

            var message = JsonSerializer.Serialize(connectionEvent);
            var subscriber = _redis.GetSubscriber();
            
            await subscriber.PublishAsync(ConnectionChannel, message);
            
            _logger.LogInformation(
                "Published connection event: {Username} from {IpAddress} - {Status}",
                username, ipAddress, isConnected ? "Connected" : "Disconnected");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing connection event to Redis");
        }
    }

    public async Task SubscribeToConnectionEventsAsync(Func<string, string, bool, Task> handler)
    {
        try
        {
            var subscriber = _redis.GetSubscriber();
            
            await subscriber.SubscribeAsync(ConnectionChannel, async (channel, message) =>
            {
                try
                {
                    var connectionEvent = JsonSerializer.Deserialize<ConnectionEvent>(message!);
                    if (connectionEvent != null)
                    {
                        await handler(connectionEvent.Username, connectionEvent.IpAddress, connectionEvent.IsConnected);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing connection event from Redis");
                }
            });

            _logger.LogInformation("Subscribed to Redis connection events channel: {Channel}", ConnectionChannel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to Redis connection events");
        }
    }
}
