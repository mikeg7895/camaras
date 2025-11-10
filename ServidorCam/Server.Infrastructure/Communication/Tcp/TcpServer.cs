using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Application.Interfaces;
using Server.Application.Interfaces.Handlers;

namespace Server.Infrastructure.Communication.Tcp;

public class TcpServer(IServiceProvider serviceProvider, ILogger<TcpServer> logger)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<TcpServer> _logger = logger;
    private TcpListener? _listener;
    private CancellationTokenSource? _cancellationTokenSource;
    private const int Port = 5000;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _listener = new TcpListener(IPAddress.Any, Port);
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        _listener.Start();
        _logger.LogInformation("TCP Server started on port {Port}", Port);

        try
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(_cancellationTokenSource.Token);
                
                // Procesar cliente en segundo plano sin bloquear
                _ = Task.Run(() => ProcessClientAsync(client, _cancellationTokenSource.Token), _cancellationTokenSource.Token);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("TCP Server stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in TCP Server");
        }
    }

    private async Task ProcessClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        var remoteEndPoint = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
        _logger.LogInformation("Client connected from {RemoteEndPoint}", remoteEndPoint);

        using var scope = _serviceProvider.CreateScope();
        var requestHandler = scope.ServiceProvider.GetRequiredService<ITcpRequestHandler>();

        try
        {
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true) { AutoFlush = true };

            while (!cancellationToken.IsCancellationRequested && client.Connected)
            {
                string? header = await ReadLineAsync(stream, cancellationToken);
                if (header == null)
                {
                    _logger.LogInformation("Client {RemoteEndPoint} closed the connection", remoteEndPoint);
                    break;
                }

                // Limpiar BOM (Byte Order Mark) y espacios
                header = header.Trim().TrimStart('\uFEFF', '\u200B');
                if (string.IsNullOrEmpty(header)) continue;

                _logger.LogInformation("Request from {RemoteEndPoint}: {Header}", remoteEndPoint, header);
                var parts = header.Split('|');
                var cmd = parts[0].ToUpperInvariant();

                string response;

                // Detectar si el comando es FRAMES|UPLOAD
                if (cmd == "FRAMES" && parts.Length > 1 && parts[1].Equals("UPLOAD", StringComparison.OrdinalIgnoreCase))
                {
                    // Obtener todos los handlers registrados
                    var handlers = scope.ServiceProvider.GetRequiredService<IEnumerable<ITcpCommandHandler>>();

                    // Buscar el que maneja el comando FRAMES
                    var framesHandler = handlers.FirstOrDefault(h => h.Command == "FRAMES");

                    if (framesHandler is null)
                    {
                        response = "ERROR|Handler FRAMES not found";
                    }
                    else
                    {
                        _logger.LogInformation("Delegating to FramesCommandHandler for {RemoteEndPoint}", remoteEndPoint);
                        response = await framesHandler.HandleAsync(parts, stream);
                    }
                    
                    _logger.LogInformation("Response to {RemoteEndPoint}: {Response}", remoteEndPoint, response);
                    await writer.WriteLineAsync(response);

                    // Cerrar conexión tras recibir un video completo
                    break;
                }
                else
                {
                    // Comandos normales (texto)
                    response = await requestHandler.HandleRequestAsync(header);
                    
                    _logger.LogInformation("Response to {RemoteEndPoint}: {Response}", remoteEndPoint, response);
                    await writer.WriteLineAsync(response);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing client {RemoteEndPoint}", remoteEndPoint);
        }
        finally
        {
            client.Close();
            _logger.LogInformation("Client disconnected: {RemoteEndPoint}", remoteEndPoint);
        }
    }


    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
        _listener?.Stop();
        _logger.LogInformation("TCP Server detenido");
    }

    private static async Task<string?> ReadLineAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var data = new List<byte>(256);
        var buffer = new byte[1];

        while (true)
        {
            int read = await stream.ReadAsync(buffer, 0, 1, cancellationToken);
            if (read == 0)
            {
                return data.Count == 0 ? null : Encoding.UTF8.GetString(data.ToArray());
            }

            byte value = buffer[0];
            if (value == (byte)'\n')
            {
                break;
            }

            if (value != (byte)'\r')
            {
                data.Add(value);
            }
        }

        return Encoding.UTF8.GetString(data.ToArray());
    }
}
