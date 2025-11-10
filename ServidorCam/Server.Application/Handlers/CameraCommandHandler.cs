using System.Net.Sockets;
using System.Text.Json;
using Server.Application.Interfaces;
using Server.Application.Interfaces.Handlers;
using Server.Core.Domain.Entities;

namespace Server.Application.Handlers;

public class CameraCommandHandler(ICameraService cameraService) : ITcpCommandHandler
{
    private readonly ICameraService _cameraService = cameraService;
    public string Command => "CAMERA";

    public async Task<string> HandleAsync(string[] args, NetworkStream? networkStream = null)
    {
        // Formato esperado:
        // CAMERA|REGISTER|name|location|userId
        // CAMERA|GET|userId
        // CAMERA|DELETE|cameraId
        // CAMERA|UPDATE|cameraId|name|location

        if (args.Length < 2)
            return "ERROR|Invalid format. Usage: CAMERA|REGISTER|name|location|userId or CAMERA|GET|userId";

        var action = args[1].ToUpperInvariant();

        return action switch
        {
            "REGISTER" => await HandleRegisterCamera(args),
            "GET" => await HandleGetCameras(args),
            "DELETE" => await HandleDeleteCamera(args),
            "UPDATE" => await HandleUpdateCamera(args),
            _ => "ERROR|Unknown action. Supported: REGISTER, GET, DELETE, UPDATE"
        };
    }

    private async Task<string> HandleRegisterCamera(string[] args)
    {
        // CAMERA|REGISTER|name|location|userId
        if (args.Length < 6)
            return "ERROR|Invalid format. Usage: CAMERA|REGISTER|name|deviceId|cameraIndex|userId";

        if (!int.TryParse(args[5], out var userId))
            return "ERROR|Invalid user ID";

        try
        {
            var camera = new Camera
            {
                Name = args[2],
                DeviceId = Guid.Parse(args[3]),
                CameraIndex = int.Parse(args[4]),
                UserId = userId,
                Status = true
            };

            await _cameraService.AddAsync(camera);
            await _cameraService.SaveChangesAsync();

            return $"SUCCESS|{JsonSerializer.Serialize(camera)}";
        }
        catch (Exception ex)
        {
            return $"ERROR|{ex.Message}";
        }
    }

    private async Task<string> HandleGetCameras(string[] args)
    {
        // CAMERA|GET|userId
        if (args.Length < 3)
            return "ERROR|Invalid format. Usage: CAMERA|GET|userId";

        if (!int.TryParse(args[2], out var userId))
            return "ERROR|Invalid user ID";

        try
        {
            var cameras = await _cameraService.GetByUserIdAsync(userId);

            return $"SUCCESS|{JsonSerializer.Serialize(cameras)}";
        }
        catch (Exception ex)
        {
            return $"ERROR|{ex.Message}";
        }
    }

    private async Task<string> HandleDeleteCamera(string[] args)
    {
        // CAMERA|DELETE|cameraId
        if (args.Length < 3)
            return "ERROR|Invalid format. Usage: CAMERA|DELETE|cameraId";

        if (!int.TryParse(args[2], out var cameraId))
            return "ERROR|Invalid camera ID";

        try
        {
            await _cameraService.Delete(cameraId);
            await _cameraService.SaveChangesAsync();
            return "SUCCESS|Camera deleted successfully";
        }
        catch (Exception ex)
        {
            return $"ERROR|{ex.Message}";
        }
    }

    private async Task<string> HandleUpdateCamera(string[] args)
    {
        // CAMERA|UPDATE|cameraId|name
        if (args.Length < 4)
            return "ERROR|Invalid format. Usage: CAMERA|UPDATE|cameraId|name";

        if (!int.TryParse(args[2], out var cameraId))
            return "ERROR|Invalid camera ID";

        try
        {
            var camera = await _cameraService.GetByIdAsync(cameraId);
            if (camera == null)
                return "ERROR|Camera not found";

            camera.Name = args[3];

            _cameraService.Update(camera);
            await _cameraService.SaveChangesAsync();

            return $"SUCCESS|{JsonSerializer.Serialize(camera)}";
        }
        catch (Exception ex)
        {
            return $"ERROR|{ex.Message}";
        }
    }
}