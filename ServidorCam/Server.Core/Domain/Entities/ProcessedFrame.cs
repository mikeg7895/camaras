namespace Server.Core.Domain.Entities;

public class ProcessedFrame
{
    public int Id { get; set; }
    public string FilePath { get; set; }
    public string FilterType { get; set; }
    public DateTime ProcessedAt { get; set; }
    public int VideoId { get; set; }
    public Video Video { get; set; }
}