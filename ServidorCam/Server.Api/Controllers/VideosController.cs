using Microsoft.AspNetCore.Mvc;
using Server.Api.DTOs;
using Server.Application.Interfaces;

namespace Server.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideosController : ControllerBase
{
    private readonly IVideoService _videoService;
    private readonly ILogger<VideosController> _logger;

    public VideosController(
        IVideoService videoService,
        ILogger<VideosController> logger)
    {
        _videoService = videoService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el listado de todos los videos con información del usuario y cámara
    /// </summary>
    /// <param name="userId">Filtrar por ID de usuario (opcional)</param>
    /// <param name="cameraId">Filtrar por ID de cámara (opcional)</param>
    /// <param name="orderBy">Ordenar por: date (fecha), size (tamaño). Default: date</param>
    /// <param name="orderDesc">Orden descendente. Default: true</param>
    /// <returns>Lista de videos con información completa</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VideoInfoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<VideoInfoResponse>>> GetAllVideos(
        [FromQuery] int? userId = null,
        [FromQuery] int? cameraId = null,
        [FromQuery] string orderBy = "date",
        [FromQuery] bool orderDesc = true)
    {
        try
        {
            IEnumerable<Server.Core.Domain.Entities.Video> videos;

            // Aplicar filtros
            if (cameraId.HasValue)
            {
                videos = await _videoService.GetByCameraIdAsync(cameraId.Value);
            }
            else if (orderBy.Equals("size", StringComparison.OrdinalIgnoreCase))
            {
                videos = await _videoService.GetVideosOrderedBySizeAsync();
                if (!orderDesc)
                {
                    videos = videos.Reverse();
                }
            }
            else
            {
                videos = await _videoService.GetAllAsync();
                videos = orderDesc 
                    ? videos.OrderByDescending(v => v.RecordedAt)
                    : videos.OrderBy(v => v.RecordedAt);
            }

            // Filtrar por usuario si se especifica
            if (userId.HasValue)
            {
                videos = videos.Where(v => v.Camera?.UserId == userId.Value);
            }

            var response = new List<VideoInfoResponse>();

            foreach (var video in videos)
            {
                var fileSize = await _videoService.GetVideoFileSizeAsync(video.FilePath);
                
                response.Add(new VideoInfoResponse
                {
                    Id = video.Id,
                    FilePath = video.FilePath,
                    RecordedAt = video.RecordedAt,
                    FileSizeBytes = fileSize,
                    FileSizeFormatted = FormatFileSize(fileSize),
                    CameraId = video.CameraId,
                    CameraName = video.Camera?.Name ?? "Unknown",
                    CameraDeviceId = video.Camera?.DeviceId ?? Guid.Empty,
                    CameraStatus = video.Camera?.Status ?? false,
                    UserId = video.Camera?.UserId ?? 0,
                    Username = video.Camera?.User?.Username ?? "Unknown",
                    UserEmail = video.Camera?.User?.Email ?? "Unknown",
                    ProcessedFramesCount = video.ProcessedFrames?.Count ?? 0
                });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener información de videos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene información de un video específico
    /// </summary>
    /// <param name="id">ID del video</param>
    /// <returns>Información detallada del video</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VideoInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<VideoInfoResponse>> GetVideoById(int id)
    {
        try
        {
            var video = await _videoService.GetByIdAsync(id);
            if (video == null)
            {
                return NotFound(new { message = $"Video con ID {id} no encontrado" });
            }

            var fileSize = await _videoService.GetVideoFileSizeAsync(video.FilePath);

            var response = new VideoInfoResponse
            {
                Id = video.Id,
                FilePath = video.FilePath,
                RecordedAt = video.RecordedAt,
                FileSizeBytes = fileSize,
                FileSizeFormatted = FormatFileSize(fileSize),
                CameraId = video.CameraId,
                CameraName = video.Camera?.Name ?? "Unknown",
                CameraDeviceId = video.Camera?.DeviceId ?? Guid.Empty,
                CameraStatus = video.Camera?.Status ?? false,
                UserId = video.Camera?.UserId ?? 0,
                Username = video.Camera?.User?.Username ?? "Unknown",
                UserEmail = video.Camera?.User?.Email ?? "Unknown",
                ProcessedFramesCount = video.ProcessedFrames?.Count ?? 0
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener información del video {VideoId}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
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
