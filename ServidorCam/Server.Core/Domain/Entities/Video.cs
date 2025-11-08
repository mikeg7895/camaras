namespace Server.Core.Domain.Entities;

public class Video
{
    public int Id { get; set; }
    public string FilePath { get; set; }
    public int FrameCount { get; set; }
    public DateTime RecordedAt { get; set; }
    public int CameraId { get; set; }
    public Camera Camera { get; set; }
    public ICollection<ProcessedFrame> ProcessedFrames { get; set; }
}