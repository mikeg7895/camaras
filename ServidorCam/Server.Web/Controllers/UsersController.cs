using Microsoft.AspNetCore.Mvc;
using Server.Application.Interfaces;
using Server.Web.Models;

namespace Server.Web.Controllers;

public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly ICameraService _cameraService;
    private readonly IVideoService _videoService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserService userService,
        ICameraService cameraService,
        IVideoService videoService,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _cameraService = cameraService;
        _videoService = videoService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var model = new UserListViewModel();

        try
        {
            var users = await _userService.GetAllUsersAsync();

            foreach (var user in users)
            {
                var cameras = await _cameraService.GetByUserIdAsync(user.Id);
                var camerasList = cameras.ToList();
                var totalVideos = camerasList.Sum(c => c.Videos?.Count() ?? 0);

                model.Users.Add(new UserViewModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Approved = user.Approved,
                    LastLogin = user.LastLogin,
                    CamerasCount = camerasList.Count,
                    TotalVideos = totalVideos
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar usuarios");
        }

        return View(model);
    }
}
