using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Server.Application.Interfaces;
using Server.Web.Models;
using Microsoft.Extensions.Configuration;

namespace Server.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IVideoService _videoService;
    private readonly ICameraService _cameraService;
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    private readonly IConnectionTrackingService _connectionTracker;

    public HomeController(
        ILogger<HomeController> logger,
        IVideoService videoService,
        ICameraService cameraService,
        IUserService userService,
        IConfiguration configuration,
        IConnectionTrackingService connectionTracker)
    {
        _logger = logger;
        _videoService = videoService;
        _cameraService = cameraService;
        _userService = userService;
        _configuration = configuration;
        _connectionTracker = connectionTracker;
    }

    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel();

        try
        {
            // Cámara con más archivos enviados
            var (camera, count) = await _videoService.GetCameraWithMostVideosAsync();
            if (camera != null)
            {
                model.CameraWithMostVideos = new CameraWithMostVideosViewModel
                {
                    CameraId = camera.Id,
                    CameraName = camera.Name,
                    OwnerUsername = camera.User?.Username ?? "Unknown",
                    VideoCount = count
                };
            }

            // Archivos enviados ordenados por tamaño
            var videos = await _videoService.GetVideosOrderedBySizeAsync();
            foreach (var video in videos.Take(20)) // Limitamos a 20 para performance
            {
                var relativePath = _configuration["FileStorage:VideoBasePath"] ?? "..\\Server.Host";
                var serverHostPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), relativePath));
                var fileSize = await _videoService.GetVideoFileSizeAsync(Path.Combine(serverHostPath, video.FilePath));
                model.Videos.Add(new VideoFileViewModel
                {
                    VideoId = video.Id,
                    FilePath = video.FilePath,
                    FileName = Path.GetFileName(video.FilePath),
                    FileSizeBytes = fileSize,
                    FileSizeFormatted = FormatFileSize(fileSize),
                    RecordedAt = video.RecordedAt,
                    CameraName = video.Camera?.Name ?? "Unknown",
                    OwnerUsername = video.Camera?.User?.Username ?? "Unknown",
                    CameraId = video.CameraId,
                    UserId = video.Camera?.UserId ?? 0
                });
            }

            // Usuarios conectados
            var connectedClients = _connectionTracker.GetConnectedClients();
            model.ConnectedClients = connectedClients.Select(c => new ConnectedClientViewModel
            {
                Username = c.Username,
                IpAddress = c.IpAddress,
                ConnectedAt = c.ConnectedAt,
                LastActivity = c.LastActivity
            }).ToList();
            
            // Usuarios recientemente desconectados
            var disconnectedClients = _connectionTracker.GetRecentlyDisconnectedClients();
            model.DisconnectedClients = disconnectedClients.Select(d => new DisconnectedClientViewModel
            {
                Username = d.Username,
                IpAddress = d.IpAddress,
                DisconnectedAt = d.DisconnectedAt,
                ConnectionDuration = d.ConnectionDuration
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar el dashboard");
        }

        return View(model);
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

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
