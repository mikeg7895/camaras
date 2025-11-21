using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using CameraClient.Desktop.Models;

namespace CameraClient.Desktop.Services;

public class CameraService
{
    private readonly TcpConnectionService _connectionService;

    public CameraService(TcpConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public async Task<CameraResponse> RegisterCameraAsync(string name, Guid deviceId, int cameraIndex, int userId)
    {
        try
        {
            // Formato: CAMERA|REGISTER|name|deviceId|cameraIndex|userId
            var command = $"CAMERA|REGISTER|{name}|{deviceId}|{cameraIndex}|{userId}";
            var response = await _connectionService.SendCommandAsync(command);
            
            if (string.IsNullOrEmpty(response))
            {
                return new CameraResponse 
                { 
                    Success = false, 
                    Message = "No response from server" 
                };
            }

            return ParseCameraResponse(response);
        }
        catch (Exception ex)
        {
            return new CameraResponse 
            { 
                Success = false, 
                Message = $"Connection error: {ex.Message}" 
            };
        }
    }

    public async Task<CameraResponse> GetCamerasAsync(int userId)
    {
        try
        {
            // Formato: CAMERA|GET|userId
            var command = $"CAMERA|GET|{userId}";
            var response = await _connectionService.SendCommandAsync(command);
            
            if (string.IsNullOrEmpty(response))
            {
                return new CameraResponse 
                { 
                    Success = false, 
                    Message = "No response from server" 
                };
            }

            return ParseCamerasListResponse(response);
        }
        catch (Exception ex)
        {
            return new CameraResponse 
            { 
                Success = false, 
                Message = $"Connection error: {ex.Message}" 
            };
        }
    }

    public async Task<CameraResponse> UpdateCameraAsync(int cameraId, string name)
    {
        try
        {
            // Formato: CAMERA|UPDATE|cameraId|name
            var command = $"CAMERA|UPDATE|{cameraId}|{name}";
            var response = await _connectionService.SendCommandAsync(command);
            
            if (string.IsNullOrEmpty(response))
            {
                return new CameraResponse 
                { 
                    Success = false, 
                    Message = "No response from server" 
                };
            }

            return ParseCameraResponse(response);
        }
        catch (Exception ex)
        {
            return new CameraResponse 
            { 
                Success = false, 
                Message = $"Connection error: {ex.Message}" 
            };
        }
    }

    public async Task<CameraResponse> DeleteCameraAsync(int cameraId)
    {
        try
        {
            // Formato: CAMERA|DELETE|cameraId
            var command = $"CAMERA|DELETE|{cameraId}";
            var response = await _connectionService.SendCommandAsync(command);
            
            if (string.IsNullOrEmpty(response))
            {
                return new CameraResponse 
                { 
                    Success = false, 
                    Message = "No response from server" 
                };
            }

            return ParseSimpleResponse(response);
        }
        catch (Exception ex)
        {
            return new CameraResponse 
            { 
                Success = false, 
                Message = $"Connection error: {ex.Message}" 
            };
        }
    }

    private CameraResponse ParseCameraResponse(string response)
    {
        // Formato de respuesta: SUCCESS|{json} o ERROR|mensaje
        var parts = response.Split('|', 2);
        
        if (parts.Length < 2)
        {
            return new CameraResponse 
            { 
                Success = false, 
                Message = "Invalid server response format" 
            };
        }

        var status = parts[0];
        var data = parts[1];

        if (status == "SUCCESS")
        {
            try
            {
                var camera = JsonSerializer.Deserialize<Camera>(data, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                return new CameraResponse
                {
                    Success = true,
                    Message = "Operation successful",
                    Camera = camera
                };
            }
            catch (JsonException ex)
            {
                return new CameraResponse
                {
                    Success = false,
                    Message = $"Failed to parse camera data: {ex.Message}"
                };
            }
        }

        return new CameraResponse
        {
            Success = false,
            Message = data
        };
    }

    private CameraResponse ParseCamerasListResponse(string response)
    {
        // Formato de respuesta: SUCCESS|[{json},{json}] o ERROR|mensaje
        var parts = response.Split('|', 2);
        
        if (parts.Length < 2)
        {
            return new CameraResponse 
            { 
                Success = false, 
                Message = "Invalid server response format" 
            };
        }

        var status = parts[0];
        var data = parts[1];

        if (status == "SUCCESS")
        {
            try
            {
                var cameras = JsonSerializer.Deserialize<List<Camera>>(data, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                }) ?? new List<Camera>();

                return new CameraResponse
                {
                    Success = true,
                    Message = $"Retrieved {cameras.Count} cameras",
                    Cameras = cameras
                };
            }
            catch (JsonException ex)
            {
                return new CameraResponse
                {
                    Success = false,
                    Message = $"Failed to parse cameras data: {ex.Message}"
                };
            }
        }

        return new CameraResponse
        {
            Success = false,
            Message = data
        };
    }

    private CameraResponse ParseSimpleResponse(string response)
    {
        // Formato de respuesta: SUCCESS|mensaje o ERROR|mensaje
        var parts = response.Split('|', 2);
        
        if (parts.Length < 2)
        {
            return new CameraResponse 
            { 
                Success = false, 
                Message = "Invalid server response format" 
            };
        }

        var status = parts[0];
        var message = parts[1];

        return new CameraResponse
        {
            Success = status == "SUCCESS",
            Message = message
        };
    }

}
