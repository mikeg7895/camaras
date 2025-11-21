using Microsoft.AspNetCore.Mvc;
using Server.Application.Interfaces;
using Server.Web.Models;
using Server.Web.Helpers;

namespace Server.Web.Controllers;

public class ProcessedFramesController : Controller
{
    private readonly IProcessedFrameService _processedFrameService;
    private readonly ILogger<ProcessedFramesController> _logger;
    private readonly IConfiguration _configuration;
    private readonly FileCacheHelper? _fileCacheHelper;

    public ProcessedFramesController(
        IProcessedFrameService processedFrameService,
        ILogger<ProcessedFramesController> logger,
        IConfiguration configuration,
        FileCacheHelper? fileCacheHelper = null)
    {
        _processedFrameService = processedFrameService;
        _logger = logger;
        _configuration = configuration;
        _fileCacheHelper = fileCacheHelper;
    }

    // GET: /ProcessedFrames?filter=Grayscale
    public async Task<IActionResult> Index(string? filter)
    {
        var model = new ProcessedFrameListViewModel
        {
            SelectedFilter = filter
        };

        try
        {
            IEnumerable<Server.Core.Domain.Entities.ProcessedFrame> frames;

            if (!string.IsNullOrEmpty(filter))
            {
                frames = await _processedFrameService.GetByFilterTypeAsync(filter);
            }
            else
            {
                frames = await _processedFrameService.GetAllAsync();
            }

            // Obtener lista de filtros únicos
            var allFrames = await _processedFrameService.GetAllAsync();
            model.AvailableFilters = allFrames
                .Select(f => f.FilterType)
                .Distinct()
                .OrderBy(f => f)
                .ToList();

            var basePath = _configuration["FileStorage:FrameBasePath"] ?? "..\\Server.Host";
            var serverHostPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), basePath));

            foreach (var frame in frames)
            {
                model.ProcessedFrames.Add(new ProcessedFrameViewModel
                {
                    Id = frame.Id,
                    FilePath = Path.Combine(serverHostPath, frame.FilePath),
                    FileName = Path.GetFileName(frame.FilePath),
                    FilterType = frame.FilterType,
                    ProcessedAt = frame.ProcessedAt,
                    VideoId = frame.VideoId,
                    VideoFilePath = frame.Video != null ? Path.Combine(serverHostPath, frame.Video.FilePath) : "",
                    VideoFileName = frame.Video != null ? Path.GetFileName(frame.Video.FilePath) : "",
                    VideoRecordedAt = frame.Video?.RecordedAt ?? DateTime.MinValue,
                    CameraId = frame.Video?.CameraId ?? 0,
                    CameraName = frame.Video?.Camera?.Name ?? "Unknown",
                    UserId = frame.Video?.Camera?.UserId ?? 0,
                    OwnerUsername = frame.Video?.Camera?.User?.Username ?? "Unknown",
                    OwnerEmail = frame.Video?.Camera?.User?.Email ?? "Unknown"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar frames procesados");
        }

        return View(model);
    }

    // GET: /ProcessedFrames/StreamFrame/5
    public async Task<IActionResult> StreamFrame(int id)
    {
        try
        {
            var frame = await _processedFrameService.GetByIdAsync(id);
            if (frame == null)
            {
                return NotFound();
            }

            // Construir la ruta absoluta a Server.Host
            var relativePath = _configuration["FileStorage:FrameBasePath"] ?? "..\\Server.Host";
            var serverHostPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), relativePath));
            var fullPath = Path.Combine(serverHostPath, frame.FilePath);

            if (!System.IO.File.Exists(fullPath))
            {
                _logger.LogWarning("Frame file not found: {FilePath}", fullPath);
                return NotFound();
            }

            byte[] fileBytes;
            
            // Intentar obtener del caché si está disponible
            if (_fileCacheHelper != null)
            {
                fileBytes = await _fileCacheHelper.GetOrCacheFrameAsync(id, fullPath) ?? Array.Empty<byte>();
                _logger.LogDebug("Serving frame {FrameId} from cache", id);
            }
            else
            {
                fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            }

            if (fileBytes.Length == 0)
            {
                return NotFound();
            }

            var fileName = Path.GetFileName(frame.FilePath);
            var contentType = GetContentType(fileName);
            return File(fileBytes, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al mostrar frame {FrameId}", id);
            return StatusCode(500);
        }
    }

    // GET: /ProcessedFrames/DownloadFrame/5
    public async Task<IActionResult> DownloadFrame(int id)
    {
        try
        {
            var frame = await _processedFrameService.GetByIdAsync(id);
            if (frame == null)
            {
                return NotFound();
            }

            // Construir la ruta absoluta a Server.Host
            var relativePath = _configuration["FileStorage:FrameBasePath"] ?? "..\\Server.Host";
            var serverHostPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), relativePath));
            var fullPath = Path.Combine(serverHostPath, frame.FilePath);

            if (!System.IO.File.Exists(fullPath))
            {
                _logger.LogWarning("Frame file not found: {FilePath}", fullPath);
                return NotFound($"Frame file not found: {Path.GetFileName(frame.FilePath)}");
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            var fileName = Path.GetFileName(frame.FilePath);
            var contentType = GetContentType(fileName);
            return File(memory, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar frame {FrameId}", id);
            return StatusCode(500);
        }
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream"
        };
    }
}
