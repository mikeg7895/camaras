using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using CameraClient.Desktop.Models;

namespace CameraClient.Desktop.Services;

public class VideoRecordingService : IDisposable
{
    private readonly TcpConnectionService _connectionService;
    private CancellationTokenSource? _recordingCancellation;
    private Task? _recordingTask;
    private bool _isRecording;

    public bool IsRecording => _isRecording;

    public VideoRecordingService(TcpConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public async Task StartRecordingAsync(Camera camera, Guid deviceId)
    {
        if (_isRecording)
        {
            Console.WriteLine("Recording is already in progress");
            return;
        }

        _recordingCancellation = new CancellationTokenSource();
        _isRecording = true;

        _recordingTask = Task.Run(async () =>
        {
            await RecordingLoopAsync(camera, deviceId, _recordingCancellation.Token);
        });

        await Task.CompletedTask;
    }

    public async Task StopRecordingAsync()
    {
        if (!_isRecording)
        {
            return;
        }

        _recordingCancellation?.Cancel();
        
        if (_recordingTask != null)
        {
            try
            {
                await _recordingTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelling
            }
        }

        _isRecording = false;
        Console.WriteLine("Recording stopped");
    }

    private async Task RecordingLoopAsync(Camera camera, Guid deviceId, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Starting recording loop for camera {camera.Name} (Index: {camera.CameraIndex})");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Grabar video de 60 segundos
                var videoPath = await RecordVideoSegmentAsync(camera.CameraIndex, 10, cancellationToken);

                if (videoPath != null && File.Exists(videoPath))
                {
                    Console.WriteLine($"Video recorded: {videoPath}");

                    // Enviar al servidor
                    await SendVideoToServerAsync(deviceId, camera.Id, videoPath);

                    // Eliminar archivo local después de enviar
                    try
                    {
                        File.Delete(videoPath);
                        Console.WriteLine($"Local video file deleted: {videoPath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting local file: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in recording loop: {ex.Message}");
                
                // Esperar un poco antes de reintentar
                if (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(5000, cancellationToken);
                }
            }
        }
    }

    private async Task<string?> RecordVideoSegmentAsync(int cameraIndex, int durationSeconds, CancellationToken cancellationToken)
    {
        VideoCapture? capture = null;
        VideoWriter? writer = null;

        try
        {
            Console.WriteLine($"Opening camera {cameraIndex} for recording...");

            // Abrir la cámara
            capture = new VideoCapture(cameraIndex, VideoCaptureAPIs.DSHOW);
            
            if (!capture.IsOpened())
            {
                Console.WriteLine($"Failed to open camera {cameraIndex}");
                return null;
            }

            // Obtener propiedades de la cámara
            var fps = capture.Get(VideoCaptureProperties.Fps);
            if (fps <= 0) fps = 30; // Default FPS

            var width = (int)capture.Get(VideoCaptureProperties.FrameWidth);
            var height = (int)capture.Get(VideoCaptureProperties.FrameHeight);

            // Crear directorio temporal
            var tempDir = Path.Combine(Path.GetTempPath(), "CameraRecordings");
            Directory.CreateDirectory(tempDir);

            // Nombre del archivo de video temporal
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var videoPath = Path.Combine(tempDir, $"camera_{cameraIndex}_{timestamp}.mp4");

            // Crear VideoWriter con codec H264
            var fourcc = VideoWriter.FourCC('H', '2', '6', '4');
            writer = new VideoWriter(videoPath, fourcc, fps, new Size(width, height));

            if (!writer.IsOpened())
            {
                Console.WriteLine("Failed to create video writer");
                return null;
            }

            Console.WriteLine($"Recording {durationSeconds} seconds at {fps}fps, {width}x{height}...");

            // Grabar frames
            var frameCount = (int)(fps * durationSeconds);
            var mat = new Mat();
            int framesWritten = 0;

            for (int i = 0; i < frameCount && !cancellationToken.IsCancellationRequested; i++)
            {
                if (!capture.Read(mat) || mat.Empty())
                {
                    Console.WriteLine($"Failed to read frame {i}");
                    break;
                }

                writer.Write(mat);
                framesWritten++;

                // No usar delay, dejar que capture a la velocidad real
                // await Task.Delay((int)(1000 / fps), cancellationToken);
            }

            mat.Dispose();

            // IMPORTANTE: Cerrar y liberar el writer ANTES de retornar
            Console.WriteLine($"Finalizing video... {framesWritten} frames written");
            writer.Release();
            writer.Dispose();
            writer = null;

            // Cerrar la cámara también
            capture.Release();
            capture.Dispose();
            capture = null;

            // Esperar un momento para asegurar que el archivo esté completamente escrito
            await Task.Delay(500, cancellationToken);

            // Verificar que el archivo existe y tiene contenido
            var fileInfo = new FileInfo(videoPath);
            if (!fileInfo.Exists)
            {
                Console.WriteLine($"ERROR: Video file was not created: {videoPath}");
                return null;
            }

            Console.WriteLine($"Recording completed: {videoPath} ({fileInfo.Length} bytes, {framesWritten} frames)");
            return videoPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error recording video: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return null;
        }
        finally
        {
            // Cleanup por si acaso
            try
            {
                writer?.Release();
                writer?.Dispose();
            }
            catch { }
            
            try
            {
                capture?.Release();
                capture?.Dispose();
            }
            catch { }
        }
    }

    private async Task SendVideoToServerAsync(Guid deviceId, int cameraId, string videoPath)
    {
        TcpClient? client = null;
        StreamWriter? writer = null;
        StreamReader? reader = null;

        try
        {
            var fileInfo = new FileInfo(videoPath);
            var fileLength = fileInfo.Length;
            Console.WriteLine($"Sending video file ({fileLength} bytes)...");

            var serverHost = _connectionService.GetServerHost();
            client = new TcpClient();
            await client.ConnectAsync(serverHost, 5000);

            var networkStream = client.GetStream();
            writer = new StreamWriter(networkStream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };
            reader = new StreamReader(networkStream, Encoding.UTF8, leaveOpen: true);

            var command = $"FRAMES|UPLOAD|{deviceId}|{cameraId}|{fileLength}";
            await writer.WriteLineAsync(command);

            await using (var fileStream = File.OpenRead(videoPath))
            {
                byte[] buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await networkStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                }
            }

            await networkStream.FlushAsync();

            var response = await reader.ReadLineAsync();
            Console.WriteLine($"Server response: {response}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending video to server: {ex.Message}");
            throw;
        }
        finally
        {
            reader?.Dispose();
            writer?.Dispose();
            client?.Close();
        }
    }

    public void Dispose()
    {
        StopRecordingAsync().Wait();
        _recordingCancellation?.Dispose();
    }
}
