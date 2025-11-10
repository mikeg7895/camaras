namespace Server.Application.Pipeline;

public class VideoContext
{
    public string VideoId { get; set; } = "";
    public string VideoPath { get; set; } = "";
    public List<string> Frames { get; set; } = new();
}
