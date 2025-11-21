namespace Server.Web.Models;

public class ProcessedFrameListViewModel
{
    public List<ProcessedFrameViewModel> ProcessedFrames { get; set; } = new();
    public List<string> AvailableFilters { get; set; } = new();
    public string? SelectedFilter { get; set; }
}

public class ProcessedFrameViewModel
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilterType { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    
    // Video info
    public int VideoId { get; set; }
    public string VideoFilePath { get; set; } = string.Empty;
    public string VideoFileName { get; set; } = string.Empty;
    public DateTime VideoRecordedAt { get; set; }
    
    // Camera info
    public int CameraId { get; set; }
    public string CameraName { get; set; } = string.Empty;
    
    // Owner info
    public int UserId { get; set; }
    public string OwnerUsername { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
}
