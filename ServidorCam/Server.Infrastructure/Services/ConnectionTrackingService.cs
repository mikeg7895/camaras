using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Logging;
using Server.Application.Interfaces;

namespace Server.Infrastructure.Services;

public class ConnectionTrackingService : IConnectionTrackingService
{
    private readonly ILogger<ConnectionTrackingService> _logger;
    private readonly ConcurrentDictionary<string, ConnectionInfo> _activeConnections;
    private readonly ConcurrentQueue<DisconnectedConnectionInfo> _recentDisconnections;
    private readonly object _disconnectionLock = new();
    private const int MaxRecentDisconnections = 50;

    private readonly IRealtimeConnectionService? _realtimeService;

    public ConnectionTrackingService(
        ILogger<ConnectionTrackingService> logger,
        IRealtimeConnectionService? realtimeService = null)
    {
        _logger = logger;
        _realtimeService = realtimeService;
        _activeConnections = new ConcurrentDictionary<string, ConnectionInfo>();
        _recentDisconnections = new ConcurrentQueue<DisconnectedConnectionInfo>();
    }

    public void RegisterConnection(string username, string ipAddress)
    {
        var key = $"{username}@{ipAddress}";
        var connectionInfo = new ConnectionInfo
        {
            Username = username,
            IpAddress = ipAddress,
            ConnectedAt = DateTime.UtcNow,
            LastActivity = DateTime.UtcNow
        };

        if (_activeConnections.TryAdd(key, connectionInfo))
        {
            _logger.LogInformation("User {Username} connected from {IpAddress}", username, ipAddress);
            
            // Publicar evento de conexión en Redis (si está disponible)
            _realtimeService?.PublishConnectionAsync(username, ipAddress, true);
        }
        else
        {
            // Actualizar última actividad si ya existe
            _activeConnections[key].LastActivity = DateTime.UtcNow;
            _logger.LogInformation("User {Username} activity updated from {IpAddress}", username, ipAddress);
        }

        RemoveUserFromDisconnections(username);
    }

    public void UnregisterConnection(string username, string ipAddress)
    {
        var key = $"{username}@{ipAddress}";
        
        if (_activeConnections.TryRemove(key, out var connectionInfo))
        {
            var disconnectionInfo = new DisconnectedConnectionInfo
            {
                Username = username,
                IpAddress = ipAddress,
                DisconnectedAt = DateTime.UtcNow,
                ConnectionDuration = DateTime.UtcNow - connectionInfo.ConnectedAt
            };

            _recentDisconnections.Enqueue(disconnectionInfo);
            
            // Mantener solo las últimas N desconexiones
            while (_recentDisconnections.Count > MaxRecentDisconnections)
            {
                _recentDisconnections.TryDequeue(out _);
            }

            _logger.LogInformation("User {Username} disconnected from {IpAddress}. Duration: {Duration}", 
                username, ipAddress, disconnectionInfo.ConnectionDuration);
            
            // Publicar evento de desconexión en Redis (si está disponible)
            _realtimeService?.PublishConnectionAsync(username, ipAddress, false);
        }
    }

    public IEnumerable<ConnectedClientInfo> GetConnectedClients()
    {
        return _activeConnections.Values.Select(c => new ConnectedClientInfo
        {
            Username = c.Username,
            IpAddress = c.IpAddress,
            ConnectedAt = c.ConnectedAt,
            LastActivity = c.LastActivity
        }).OrderByDescending(c => c.ConnectedAt);
    }

    public IEnumerable<DisconnectedClientInfo> GetRecentlyDisconnectedClients()
    {
        return _recentDisconnections.Select(d => new DisconnectedClientInfo
        {
            Username = d.Username,
            IpAddress = d.IpAddress,
            DisconnectedAt = d.DisconnectedAt,
            ConnectionDuration = d.ConnectionDuration
        }).OrderByDescending(d => d.DisconnectedAt);
    }

    public bool IsUserConnected(string username)
    {
        return _activeConnections.Values.Any(c => c.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    private void RemoveUserFromDisconnections(string username)
    {
        if (_recentDisconnections.IsEmpty)
        {
            return;
        }

        lock (_disconnectionLock)
        {
            var snapshot = _recentDisconnections.ToArray();
            if (!snapshot.Any(d => d.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            while (_recentDisconnections.TryDequeue(out _))
            {
                // vaciar cola
            }

            foreach (var entry in snapshot.Where(d => !d.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                _recentDisconnections.Enqueue(entry);
            }
        }
    }


    private class ConnectionInfo
    {
        public string Username { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime ConnectedAt { get; set; }
        public DateTime LastActivity { get; set; }
    }

    private class DisconnectedConnectionInfo
    {
        public string Username { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime DisconnectedAt { get; set; }
        public TimeSpan ConnectionDuration { get; set; }
    }
}
