using Microsoft.AspNetCore.Mvc;
using Server.Api.DTOs;
using Server.Application.Interfaces;

namespace Server.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
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

    /// <summary>
    /// Obtiene información completa de todos los usuarios
    /// </summary>
    /// <returns>Lista de usuarios con información detallada</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserInfoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<UserInfoResponse>>> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            var response = new List<UserInfoResponse>();

            foreach (var user in users)
            {
                var cameras = await _cameraService.GetByUserIdAsync(user.Id);
                var userResponse = new UserInfoResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Approved = user.Approved,
                    LastLogin = user.LastLogin,
                    TotalCameras = cameras.Count(),
                    TotalVideos = 0,
                    Cameras = new List<CameraBasicInfo>()
                };

                foreach (var camera in cameras)
                {
                    var videos = await _videoService.GetByCameraIdAsync(camera.Id);
                    var videoCount = videos.Count();
                    userResponse.TotalVideos += videoCount;

                    userResponse.Cameras.Add(new CameraBasicInfo
                    {
                        Id = camera.Id,
                        Name = camera.Name,
                        DeviceId = camera.DeviceId,
                        Status = camera.Status,
                        VideoCount = videoCount
                    });
                }

                response.Add(userResponse);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener información de usuarios");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene información completa de un usuario específico
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <returns>Información detallada del usuario</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserInfoResponse>> GetUserById(int id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = $"Usuario con ID {id} no encontrado" });
            }

            var cameras = await _cameraService.GetByUserIdAsync(user.Id);
            var userResponse = new UserInfoResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Approved = user.Approved,
                LastLogin = user.LastLogin,
                TotalCameras = cameras.Count(),
                TotalVideos = 0,
                Cameras = new List<CameraBasicInfo>()
            };

            foreach (var camera in cameras)
            {
                var videos = await _videoService.GetByCameraIdAsync(camera.Id);
                var videoCount = videos.Count();
                userResponse.TotalVideos += videoCount;

                userResponse.Cameras.Add(new CameraBasicInfo
                {
                    Id = camera.Id,
                    Name = camera.Name,
                    DeviceId = camera.DeviceId,
                    Status = camera.Status,
                    VideoCount = videoCount
                });
            }

            return Ok(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener información del usuario {UserId}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
