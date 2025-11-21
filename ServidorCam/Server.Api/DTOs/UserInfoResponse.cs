namespace Server.Api.DTOs;

public class UserInfoResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Approved { get; set; }
    public DateTime LastLogin { get; set; }
    public int TotalCameras { get; set; }
    public int TotalVideos { get; set; }
    public List<CameraBasicInfo> Cameras { get; set; } = new();
}

public class CameraBasicInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid DeviceId { get; set; }
    public bool Status { get; set; }
    public int VideoCount { get; set; }
}
