using Server.Application.Interfaces;
using Server.Application.Interfaces.Handlers;
using Server.Core.Domain.Entities;
using System.Net.Sockets;

namespace Server.Application.Handlers;

public class FramesCommandHandler(IVideoService videoService, ICameraService cameraService, IVideoProcessingService videoProcessingService) : ITcpCommandHandler
{
    private readonly IVideoService _videoService = videoService;
    private readonly ICameraService cameraService = cameraService;
    private readonly IVideoProcessingService _videoProcessingService = videoProcessingService;
    public string Command => "FRAMES";

    public async Task<string> HandleAsync(string[] args, NetworkStream? stream)
    {
        if (stream is null) return "ERROR required stream";
        if (args.Length < 5)
            return "ERROR|Usage: FRAMES|UPLOAD|deviceId|cameraId|length";

        var deviceId = args[2];
        var cameraId = int.Parse(args[3]);
        long length = long.Parse(args[4]);

        var camera = await cameraService.GetByIdAsync(cameraId);
        if (camera is null) return "ERROR|Camera not found";
        if (camera.DeviceId.ToString() != deviceId)
            return "ERROR|Camera does not belong to the specified device";

        string dir = Path.Combine("videos", DateTime.Now.ToString("yyyyMMdd"));
        Directory.CreateDirectory(dir);

        string timestamp = DateTime.Now.ToString("HHmmss_fff"); 
        string filePath = Path.Combine(dir, $"{cameraId}_{timestamp}.mp4");
        
        Console.WriteLine($"Receiving video: {filePath}");
        Console.WriteLine($"Expected size: {length} bytes");

        long total = 0;
        
        // Usar using para asegurar que el archivo se cierre correctamente
        await using (var fs = File.Create(filePath))
        {
            byte[] buffer = new byte[8192];

            while (total < length)
            {
                int toRead = (int)Math.Min(buffer.Length, length - total);
                int read = await stream.ReadAsync(buffer, 0, toRead);
                
                if (read == 0)
                {
                    Console.WriteLine($"Connection closed prematurely. Received {total}/{length} bytes");
                    break;
                }
                
                await fs.WriteAsync(buffer, 0, read);
                total += read;
                
                // Log cada 1MB
                if (total % (1024 * 1024) == 0 || total == length)
                {
                    Console.WriteLine($"Progress: {total}/{length} bytes ({(total * 100.0 / length):F1}%)");
                }
            }
            
            await fs.FlushAsync();
        } // El archivo se cierra aquí automáticamente
        
        Console.WriteLine($"Video received: {total} bytes. File saved: {filePath}");
        
        // Verificar que el archivo existe y tiene el tamaño correcto
        var fileInfo = new FileInfo(filePath);
        if (!fileInfo.Exists)
        {
            return $"ERROR|File was not created: {filePath}";
        }
        
        Console.WriteLine($"File size on disk: {fileInfo.Length} bytes");
        
        if (fileInfo.Length != total)
        {
            Console.WriteLine($"WARNING: File size mismatch. Expected {total}, got {fileInfo.Length}");
        }

        // Guardar en BD
        var video = await _videoService.AddAsync(new Video
        {
            FilePath = filePath,
            RecordedAt = DateTime.Now,
            CameraId = cameraId,
            FrameCount = 0
        });
        await _videoService.SaveChangesAsync();

        Console.WriteLine($"Video saved to database with ID: {video.Id}");

        // Encolar para procesamiento
        _videoProcessingService.Enqueue(video.Id.ToString(), filePath);

        return $"OK|Received {total} bytes";
    }

}