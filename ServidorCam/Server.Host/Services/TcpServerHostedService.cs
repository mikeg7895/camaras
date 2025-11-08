using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Infrastructure.Communication.Tcp;

namespace Server.Host.Services;

public class TcpServerHostedService : BackgroundService
{
    private readonly TcpServer _tcpServer;
    private readonly ILogger<TcpServerHostedService> _logger;

    public TcpServerHostedService(TcpServer tcpServer, ILogger<TcpServerHostedService> logger)
    {
        _tcpServer = tcpServer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando TCP Server Background Service...");
        
        try
        {
            await _tcpServer.StartAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en TCP Server Background Service");
            throw;
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deteniendo TCP Server Background Service...");
        _tcpServer.Stop();
        return base.StopAsync(cancellationToken);
    }
}
