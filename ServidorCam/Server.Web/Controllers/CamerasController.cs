using Microsoft.AspNetCore.Mvc;
using Server.Application.Interfaces;
using Server.Web.Models;

namespace Server.Web.Controllers;

public class CamerasController : Controller
{
    private readonly ICameraService _cameraService;
    private readonly IUserService _userService;
    private readonly IVideoService _videoService;
    private readonly ILogger<CamerasController> _logger;
    private readonly IConfiguration _configuration;

    public CamerasController(
        ICameraService cameraService,
        IUserService userService,
        IVideoService videoService,
        ILogger<CamerasController> logger,
        IConfiguration configuration)
    {
        _cameraService = cameraService;
        _userService = userService;
        _videoService = videoService;
        _logger = logger;
        _configuration = configuration;
    }

    // GET: /Cameras?userId=1
    public async Task<IActionResult> Index(int? userId)
    {
        var model = new CameraListViewModel();

        try
        {
            if (userId.HasValue)
            {
                var user = await _userService.GetByIdAsync(userId.Value);
                if (user != null)
                {
                    model.UserId = user.Id;
                    model.Username = user.Username;

                    var cameras = await _cameraService.GetByUserIdAsync(userId.Value);
                    foreach (var camera in cameras)
                    {
                        model.Cameras.Add(new CameraViewModel
                        {
                            Id = camera.Id,
                            Name = camera.Name,
                            DeviceId = camera.DeviceId,
                            CameraIndex = camera.CameraIndex,
                            Status = camera.Status,
                            VideosCount = camera.Videos?.Count ?? 0,
                            OwnerUsername = camera.User?.Username ?? user.Username
                        });
                    }
                }
            }
            else
            {
                // Mostrar todas las cámaras
                var allCameras = await _cameraService.GetAllAsync();
                foreach (var camera in allCameras)
                {
                    model.Cameras.Add(new CameraViewModel
                    {
                        Id = camera.Id,
                        Name = camera.Name,
                        DeviceId = camera.DeviceId,
                        CameraIndex = camera.CameraIndex,
                        Status = camera.Status,
                        VideosCount = camera.Videos?.Count ?? 0,
                        OwnerUsername = camera.User?.Username ?? "Unknown"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar cámaras");
        }

        return View(model);
    }

    // GET: /Cameras/Details/5
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var camera = await _cameraService.GetCameraWithDetailsAsync(id);
            if (camera == null)
            {
                return NotFound();
            }

            var model = new CameraDetailViewModel
            {
                Id = camera.Id,
                Name = camera.Name,
                DeviceId = camera.DeviceId,
                CameraIndex = camera.CameraIndex,
                Status = camera.Status,
                OwnerUsername = camera.User?.Username ?? "Unknown",
                OwnerEmail = camera.User?.Email ?? "Unknown",
                TotalVideos = camera.Videos?.Count ?? 0,
                TotalProcessedFrames = camera.Videos?.Sum(v => v.ProcessedFrames?.Count ?? 0) ?? 0
            };

            // Cargar videos con información de tamaño
            var videos = await _videoService.GetByCameraIdAsync(id);
            var relativePath = _configuration["FileStorage:VideoBasePath"] ?? "..\\Server.Host";
            var serverHostPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), relativePath));

            foreach (var video in videos)
            {
                var fileSize = await _videoService.GetVideoFileSizeAsync(Path.Combine(serverHostPath, video.FilePath));
                model.Videos.Add(new VideoFileViewModel
                {
                    VideoId = video.Id,
                    FilePath = Path.Combine(serverHostPath, video.FilePath),
                    FileName = Path.GetFileName(video.FilePath),
                    FileSizeBytes = fileSize,
                    FileSizeFormatted = FormatFileSize(fileSize),
                    RecordedAt = video.RecordedAt,
                    CameraName = camera.Name,
                    OwnerUsername = camera.User?.Username ?? "Unknown",
                    CameraId = camera.Id,
                    UserId = camera.UserId
                });
            }

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar detalles de cámara {CameraId}", id);
            return StatusCode(500);
        }
    }

    // GET: /Cameras/StreamVideo/5
    public async Task<IActionResult> StreamVideo(int id)
    {
        try
        {
            var video = await _videoService.GetByIdAsync(id);
            if (video == null)
            {
                return NotFound();
            }

            // Construir la ruta absoluta a Server.Host
            var relativePath = _configuration["FileStorage:VideoBasePath"] ?? "..\\Server.Host";
            var serverHostPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), relativePath));
            var fullPath = Path.Combine(serverHostPath, video.FilePath);

            if (!System.IO.File.Exists(fullPath))
            {
                _logger.LogWarning("Video file not found: {FilePath}", fullPath);
                return NotFound();
            }

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(stream, "video/mp4", enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al reproducir video {VideoId}", id);
            return StatusCode(500);
        }
    }

    // GET: /Cameras/DownloadVideo/5
    public async Task<IActionResult> DownloadVideo(int id)
    {
        try
        {
            var video = await _videoService.GetByIdAsync(id);
            if (video == null)
            {
                return NotFound();
            }

            // Construir la ruta absoluta a Server.Host
            var relativePath = _configuration["FileStorage:VideoBasePath"] ?? "..\\Server.Host\\uploads";
            var serverHostPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), relativePath));
            var fullPath = Path.Combine(serverHostPath, video.FilePath);

            if (!System.IO.File.Exists(fullPath))
            {
                _logger.LogWarning("Video file not found: {FilePath}", fullPath);
                return NotFound($"Video file not found: {Path.GetFileName(video.FilePath)}");
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            var fileName = Path.GetFileName(video.FilePath);
            return File(memory, "video/mp4", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar video {VideoId}", id);
            return StatusCode(500);
        }
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
