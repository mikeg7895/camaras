using Server.Core.Domain.Entities;

namespace Server.Web.Models;

public class UserListViewModel
{
    public List<UserViewModel> Users { get; set; } = new();
}

public class UserViewModel
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Approved { get; set; }
    public DateTime LastLogin { get; set; }
    public int CamerasCount { get; set; }
    public int TotalVideos { get; set; }
}
