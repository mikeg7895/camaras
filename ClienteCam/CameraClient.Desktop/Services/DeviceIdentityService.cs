using System;
using System.IO;

namespace CameraClient.Desktop.Services;

public class DeviceIdentityService
{
    private const string IdentityFileName = "device_identity.dat";
    private static readonly string IdentityFilePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, 
        IdentityFileName);
    
    private static Guid? _cachedDeviceId;

    public static Guid GetOrCreateDeviceId()
    {
        // Si ya está en caché, retornar
        if (_cachedDeviceId.HasValue)
            return _cachedDeviceId.Value;

        // Verificar si el archivo existe
        if (File.Exists(IdentityFilePath))
        {
            try
            {
                // Leer el GUID del archivo
                var guidString = File.ReadAllText(IdentityFilePath).Trim();
                if (Guid.TryParse(guidString, out var deviceId))
                {
                    _cachedDeviceId = deviceId;
                    Console.WriteLine($"Device ID loaded: {deviceId}");
                    return deviceId;
                }
                
                // Si el archivo está corrupto, generar uno nuevo
                Console.WriteLine("Device ID file corrupted, generating new one");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading device ID: {ex.Message}");
            }
        }

        // Generar un nuevo GUID
        var newDeviceId = Guid.NewGuid();
        
        try
        {
            // Guardar el GUID en el archivo
            File.WriteAllText(IdentityFilePath, newDeviceId.ToString());
            _cachedDeviceId = newDeviceId;
            
            Console.WriteLine($"New Device ID created and saved: {newDeviceId}");
            Console.WriteLine($"Saved to: {IdentityFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving device ID: {ex.Message}");
        }

        return newDeviceId;
    }

    public static string GetDeviceIdFilePath()
    {
        return IdentityFilePath;
    }

    public static bool DeviceIdFileExists()
    {
        return File.Exists(IdentityFilePath);
    }

    public static void ResetDeviceId()
    {
        try
        {
            if (File.Exists(IdentityFilePath))
            {
                File.Delete(IdentityFilePath);
                _cachedDeviceId = null;
                Console.WriteLine("Device ID reset successfully");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error resetting device ID: {ex.Message}");
        }
    }
}
