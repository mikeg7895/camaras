using Server.Core.Domain.Entities;

namespace Server.Web.Models;

public class DashboardViewModel
{
    public CameraWithMostVideosViewModel? CameraWithMostVideos { get; set; }
    public List<VideoFileViewModel> Videos { get; set; } = new();
    public List<ConnectedClientViewModel> ConnectedClients { get; set; } = new();
    public List<DisconnectedClientViewModel> DisconnectedClients { get; set; } = new();
}

public class CameraWithMostVideosViewModel
{
    public int CameraId { get; set; }
    public string CameraName { get; set; } = string.Empty;
    public string OwnerUsername { get; set; } = string.Empty;
    public int VideoCount { get; set; }
}

public class VideoFileViewModel
{
    public int VideoId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string FileSizeFormatted { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public string CameraName { get; set; } = string.Empty;
    public string OwnerUsername { get; set; } = string.Empty;
    public int CameraId { get; set; }
    public int UserId { get; set; }
}

public class ConnectedClientViewModel
{
    public string Username { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }
    public DateTime LastActivity { get; set; }
}

public class DisconnectedClientViewModel
{
    public string Username { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime DisconnectedAt { get; set; }
    public TimeSpan ConnectionDuration { get; set; }
}
