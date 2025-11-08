namespace Server.Core.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public bool Approved { get; set; }
    public DateTime LastLogin { get; set; }

    public ICollection<Camera> Cameras { get; set; }
}