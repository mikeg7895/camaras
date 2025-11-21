namespace Server.Application.Interfaces;

public interface IRealtimeConnectionService
{
    Task PublishConnectionAsync(string username, string ipAddress, bool isConnected);
    Task SubscribeToConnectionEventsAsync(Func<string, string, bool, Task> handler);
}

public class ConnectionEvent
{
    public string Username { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
