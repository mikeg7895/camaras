namespace Server.Api.DTOs;

public class VideoInfoResponse
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public long FileSizeBytes { get; set; }
    public string FileSizeFormatted { get; set; } = string.Empty;
    
    // Información de la cámara
    public int CameraId { get; set; }
    public string CameraName { get; set; } = string.Empty;
    public Guid CameraDeviceId { get; set; }
    public bool CameraStatus { get; set; }
    
    // Información del usuario propietario
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    
    // Frames procesados
    public int ProcessedFramesCount { get; set; }
}
