namespace Server.Application.Interfaces;

public interface IConnectionTrackingService
{
    void RegisterConnection(string username, string ipAddress);
    void UnregisterConnection(string username, string ipAddress);
    IEnumerable<ConnectedClientInfo> GetConnectedClients();
    IEnumerable<DisconnectedClientInfo> GetRecentlyDisconnectedClients();
    bool IsUserConnected(string username);
}

public class ConnectedClientInfo
{
    public string Username { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }
    public DateTime LastActivity { get; set; }
}

public class DisconnectedClientInfo
{
    public string Username { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime DisconnectedAt { get; set; }
    public TimeSpan ConnectionDuration { get; set; }
}
