using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OpenCvSharp;

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
        
        // Intentar detectar hasta 10 cámaras
        for (int i = 0; i < 10; i++)
        {
            VideoCapture? capture = null;
            try
            {
                // Intentar abrir la cámara con diferentes backends
                capture = TryOpenCamera(i);
                
                if (capture != null && capture.IsOpened())
                {
                    // Obtener propiedades de la cámara
                    var width = capture.Get(VideoCaptureProperties.FrameWidth);
                    var height = capture.Get(VideoCaptureProperties.FrameHeight);
                    var fps = capture.Get(VideoCaptureProperties.Fps);
                    
                    cameras.Add(new DetectedCamera
                    {
                        Index = i,
                        Name = $"Camera {i}",
                        Description = $"{GetCameraTypeName(i)} - {width}x{height} @ {fps:F0}fps"
                    });
                    
                    Console.WriteLine($"✓ Detected camera {i}: {width}x{height}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Camera {i} not available: {ex.Message}");
            }
            finally
            {
                try
                {
                    capture?.Release();
                    capture?.Dispose();
                }
                catch { }
            }
        }
        
        return cameras;
    }

    private static VideoCapture? TryOpenCamera(int index)
    {
        // Lista de backends a intentar en orden de preferencia
        var backends = new[]
        {
            VideoCaptureAPIs.V4L2,      // Linux
            VideoCaptureAPIs.DSHOW,     // Windows DirectShow
            VideoCaptureAPIs.MSMF,      // Windows Media Foundation
            VideoCaptureAPIs.AVFOUNDATION, // macOS
            VideoCaptureAPIs.ANY        // Cualquier backend disponible
        };

        foreach (var backend in backends)
        {
            try
            {
                var capture = new VideoCapture(index, backend);
                
                // Dar tiempo para que se inicialice
                System.Threading.Thread.Sleep(50);
                
                if (capture.IsOpened())
                {
                    Console.WriteLine($"Camera {index} opened with backend: {backend}");
                    return capture;
                }
                
                capture.Dispose();
            }
            catch
            {
                // Probar siguiente backend
            }
        }

        return null;
    }

    private static string GetCameraTypeName(int index)
    {
        return index switch
        {
            0 => "Built-in Webcam",
            1 => "USB Camera",
            2 => "External Camera",
            _ => $"Camera Device {index}"
        };
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
            capture = TryOpenCamera(cameraIndex);
            return capture != null && capture.IsOpened();
        }
        catch
        {
            // Fallback: verificar por sistema operativo
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var devicePath = $"/dev/video{cameraIndex}";
                return File.Exists(devicePath);
            }
            
            return cameraIndex >= 0 && cameraIndex < 5;
        }
        finally
        {
            try
            {
                capture?.Release();
                capture?.Dispose();
            }
            catch { }
        }
    }
}
