using System.Collections.Generic;

namespace CameraClient.Desktop.Models;

public class CameraResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Camera? Camera { get; set; }
    public List<Camera>? Cameras { get; set; }
}
