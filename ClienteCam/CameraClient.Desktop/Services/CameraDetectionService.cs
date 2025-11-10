using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OpenCvSharp;
using System.Management;

namespace CameraClient.Desktop.Services;

public class CameraDetectionService
{
    public class DetectedCamera
    {
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public static Task<List<DetectedCamera>> GetAvailableCamerasAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                return DetectCamerasWithOpenCV();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing OpenCV: {ex.Message}");
                Console.WriteLine($"Falling back to system detection");
                
                // Fallback a detección por sistema operativo
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return DetectLinuxCamerasManually();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return GetSimulatedCameras();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return GetSimulatedCameras();
                }
                
                return new List<DetectedCamera>();
            }
        });
    }

    private static List<DetectedCamera> DetectCamerasWithOpenCV()
    {
        var cameras = new List<DetectedCamera>();
        
        Console.WriteLine("Starting camera detection with OpenCV...");
        
        // Obtener nombres reales de las cámaras desde Windows
        var cameraNames = GetWindowsCameraNames();
        
        // En Windows, intentar detectar hasta 5 cámaras (raramente hay más)
        for (int i = 0; i < 5; i++)
        {
            VideoCapture? capture = null;
            try
            {
                Console.WriteLine($"Attempting to detect camera {i}...");
                
                // Intentar abrir la cámara con diferentes backends
                capture = TryOpenCamera(i);
                
                if (capture != null && capture.IsOpened())
                {
                    // Obtener propiedades de la cámara
                    var width = capture.Get(VideoCaptureProperties.FrameWidth);
                    var height = capture.Get(VideoCaptureProperties.FrameHeight);
                    var fps = capture.Get(VideoCaptureProperties.Fps);
                    
                    // Si FPS es 0, intentar leer un frame para obtener info real
                    if (fps == 0)
                    {
                        fps = 30; // Valor por defecto común
                    }
                    
                    // Obtener el nombre real de la cámara
                    string cameraName = cameraNames.Count > i ? cameraNames[i] : $"Camera {i}";
                    string description = $"{width:F0}x{height:F0} @ {fps:F0}fps";
                    
                    cameras.Add(new DetectedCamera
                    {
                        Index = i,
                        Name = cameraName,
                        Description = description
                    });
                    
                    Console.WriteLine($"✓ Detected camera {i}: {cameraName} - {description}");
                }
                else
                {
                    Console.WriteLine($"✗ Camera {i} not available");
                    // Si no encontramos cámaras consecutivas, probablemente no hay más
                    if (cameras.Count == 0 && i >= 2)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Camera {i} error: {ex.Message}");
                // Si fallaron las primeras 2 cámaras, probablemente no hay ninguna
                if (i >= 2 && cameras.Count == 0)
                {
                    break;
                }
            }
            finally
            {
                try
                {
                    capture?.Release();
                    capture?.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error releasing camera {i}: {ex.Message}");
                }
            }
            
            // Pequeña pausa entre detecciones para evitar problemas
            System.Threading.Thread.Sleep(100);
        }
        
        Console.WriteLine($"Camera detection completed. Found {cameras.Count} camera(s)");
        return cameras;
    }

    private static VideoCapture? TryOpenCamera(int index)
    {
        // Lista de backends a intentar en orden de preferencia para Windows
        var backends = new[]
        {
            VideoCaptureAPIs.DSHOW,     // Windows DirectShow (más compatible)
            VideoCaptureAPIs.MSMF,      // Windows Media Foundation (más moderno)
            VideoCaptureAPIs.ANY        // Cualquier backend disponible
        };

        foreach (var backend in backends)
        {
            try
            {
                var capture = new VideoCapture(index, backend);
                
                // Dar más tiempo para que se inicialice en Windows
                System.Threading.Thread.Sleep(200);
                
                if (capture.IsOpened())
                {
                    // Verificar que realmente puede capturar frames
                    using var testFrame = new Mat();
                    if (capture.Read(testFrame) && !testFrame.Empty())
                    {
                        Console.WriteLine($"Camera {index} opened successfully with backend: {backend}");
                        return capture;
                    }
                }
                
                capture.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open camera {index} with {backend}: {ex.Message}");
            }
        }

        return null;
    }

    private static List<string> GetWindowsCameraNames()
    {
        var cameraNames = new List<string>();
        
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return cameraNames;
        }

        try
        {
            // Usar WMI para obtener los nombres reales de las cámaras en Windows
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Image' OR PNPClass = 'Camera')");
            
            foreach (ManagementObject device in searcher.Get())
            {
                var name = device["Caption"]?.ToString();
                if (!string.IsNullOrEmpty(name))
                {
                    cameraNames.Add(name);
                    Console.WriteLine($"Found Windows camera device: {name}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting Windows camera names via WMI: {ex.Message}");
            
            // Intentar método alternativo usando DirectShow
            try
            {
                cameraNames = GetCameraNamesViaDirectShow();
            }
            catch (Exception ex2)
            {
                Console.WriteLine($"Error getting camera names via DirectShow: {ex2.Message}");
            }
        }

        return cameraNames;
    }

    private static List<string> GetCameraNamesViaDirectShow()
    {
        var cameraNames = new List<string>();
        
        // Este método intenta obtener nombres mediante DirectShow
        // Como alternativa si WMI falla
        for (int i = 0; i < 5; i++)
        {
            try
            {
                using var capture = new VideoCapture(i, VideoCaptureAPIs.DSHOW);
                if (capture.IsOpened())
                {
                    // DirectShow no proporciona el nombre directamente,
                    // así que usamos un nombre genérico basado en el índice
                    cameraNames.Add($"Camera {i}");
                    capture.Release();
                }
            }
            catch
            {
                break;
            }
        }
        
        return cameraNames;
    }

    private static List<DetectedCamera> DetectLinuxCamerasManually()
    {
        var cameras = new List<DetectedCamera>();
        var videoDevicesPath = "/dev";

        try
        {
            if (!Directory.Exists(videoDevicesPath))
            {
                Console.WriteLine("Warning: /dev directory not found");
                return GetSimulatedCameras();
            }

            // Buscar dispositivos de video (/dev/video0, /dev/video1, etc.)
            var videoFiles = Directory.GetFiles(videoDevicesPath, "video*")
                .Where(f =>
                {
                    var name = Path.GetFileName(f).Replace("video", "");
                    return !string.IsNullOrEmpty(name) && char.IsDigit(name[0]);
                })
                .OrderBy(f => f)
                .ToList();

            if (videoFiles.Count == 0)
            {
                Console.WriteLine("Warning: No video devices found in /dev. Using simulated cameras.");
                return GetSimulatedCameras();
            }

            foreach (var videoFile in videoFiles)
            {
                try
                {
                    var fileName = Path.GetFileName(videoFile);
                    var deviceIndex = int.Parse(fileName.Replace("video", ""));
                    
                    if (File.Exists(videoFile))
                    {
                        var description = GetLinuxCameraInfo(videoFile);
                        
                        cameras.Add(new DetectedCamera
                        {
                            Index = deviceIndex,
                            Name = $"Camera {deviceIndex}",
                            Description = description
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error detecting camera {videoFile}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scanning for cameras: {ex.Message}");
            return GetSimulatedCameras();
        }

        return cameras.Count > 0 ? cameras : GetSimulatedCameras();
    }

    private static List<DetectedCamera> GetSimulatedCameras()
    {
        // Cámaras simuladas para desarrollo/testing
        return new List<DetectedCamera>
        {
            new DetectedCamera { Index = 0, Name = "Camera 0", Description = "Simulated Webcam (640x480)" },
            new DetectedCamera { Index = 1, Name = "Camera 1", Description = "Simulated USB Camera (1280x720)" },
            new DetectedCamera { Index = 2, Name = "Camera 2", Description = "Simulated External Camera (1920x1080)" }
        };
    }

    private static string GetLinuxCameraInfo(string devicePath)
    {
        try
        {
            var deviceName = Path.GetFileName(devicePath);
            var sysfsPath = $"/sys/class/video4linux/{deviceName}/name";
            
            if (File.Exists(sysfsPath))
            {
                var name = File.ReadAllText(sysfsPath).Trim();
                return string.IsNullOrEmpty(name) ? devicePath : name;
            }
        }
        catch
        {
            // Ignorar errores
        }

        return devicePath;
    }

    public static bool IsCameraAvailable(int cameraIndex)
    {
        VideoCapture? capture = null;
        try
        {
            Console.WriteLine($"Checking availability of camera {cameraIndex}...");
            capture = TryOpenCamera(cameraIndex);
            var isAvailable = capture != null && capture.IsOpened();
            Console.WriteLine($"Camera {cameraIndex} is {(isAvailable ? "available" : "not available")}");
            return isAvailable;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking camera {cameraIndex}: {ex.Message}");
            return false;
        }
        finally
        {
            try
            {
                capture?.Release();
                capture?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error releasing camera {cameraIndex}: {ex.Message}");
            }
        }
    }
}
