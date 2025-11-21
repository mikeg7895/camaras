namespace Server.Web.Models;

public class CameraListViewModel
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public List<CameraViewModel> Cameras { get; set; } = new();
}

public class CameraViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid DeviceId { get; set; }
    public int CameraIndex { get; set; }
    public bool Status { get; set; }
    public string StatusText => Status ? "Activa" : "Inactiva";
    public int VideosCount { get; set; }
    public string OwnerUsername { get; set; } = string.Empty;
}

public class CameraDetailViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid DeviceId { get; set; }
    public int CameraIndex { get; set; }
    public bool Status { get; set; }
    public string StatusText => Status ? "Activa" : "Inactiva";
    public string OwnerUsername { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public List<VideoFileViewModel> Videos { get; set; } = new();
    public int TotalVideos { get; set; }
    public int TotalProcessedFrames { get; set; }
}
