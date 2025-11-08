namespace Server.Core.Domain.Entities;

public class Camera
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Guid DeviceId { get; set; }
    public int CameraIndex { get; set; }
    public bool Status { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public ICollection<Video> Videos { get; set; }
}