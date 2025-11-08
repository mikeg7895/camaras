using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Application.Interfaces;

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
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            _logger.LogDebug("Stream configured for {RemoteEndPoint}, waiting for messages", remoteEndPoint);

            while (!cancellationToken.IsCancellationRequested && client.Connected)
            {
                try
                {
                    var request = await reader.ReadLineAsync(cancellationToken);
                    
                    // Si ReadLineAsync retorna null, el cliente cerró la conexión
                    if (request == null)
                    {
                        _logger.LogInformation("Client {RemoteEndPoint} closed the connection", remoteEndPoint);
                        break;
                    }

                    request = request.Trim();
                    if (string.IsNullOrEmpty(request))
                        continue;

                    _logger.LogInformation("Request from {RemoteEndPoint}: {Request}", remoteEndPoint, request);

                    var response = await requestHandler.HandleRequestAsync(request);
                    
                    _logger.LogInformation("Response to {RemoteEndPoint}: {Response}", remoteEndPoint, response);

                    await writer.WriteLineAsync(response);
                }
                catch (IOException ioEx)
                {
                    _logger.LogWarning("Client {RemoteEndPoint} disconnected abruptly: {Message}", remoteEndPoint, ioEx.Message);
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Connection cancelled with {RemoteEndPoint}", remoteEndPoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing client {RemoteEndPoint}", remoteEndPoint);
        }
        finally
        {
            try
            {
                client.Close();
            }
            catch { }
            
            _logger.LogInformation("Client disconnected: {RemoteEndPoint}", remoteEndPoint);
        }
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
        _listener?.Stop();
        _logger.LogInformation("TCP Server detenido");
    }
}