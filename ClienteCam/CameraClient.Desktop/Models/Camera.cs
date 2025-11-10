using System;
using ReactiveUI;

namespace CameraClient.Desktop.Models;

public class Camera : ReactiveObject
{
    private bool _isRecording;

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid DeviceId { get; set; }
    public int CameraIndex { get; set; }
    public bool Status { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Helper properties para UI
    public bool BelongsToThisDevice { get; set; }
    public string DeviceIdShort => DeviceId.ToString().Substring(0, 8);

    public bool IsRecording
    {
        get => _isRecording;
        set => this.RaiseAndSetIfChanged(ref _isRecording, value);
    }
}
