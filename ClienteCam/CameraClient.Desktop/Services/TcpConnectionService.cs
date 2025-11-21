using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CameraClient.Desktop.Services;

/// <summary>
/// Servicio que mantiene una conexión TCP persistente con el servidor
/// </summary>
public class TcpConnectionService : IDisposable
{
    private const int ServerPort = 5000;
    private string _serverHost = "localhost"; // Valor por defecto
    
    private TcpClient? _client;
    private NetworkStream? _stream;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private bool _isAuthenticated = false;

    public bool IsConnected => _client?.Connected == true;
    public bool IsAuthenticated => _isAuthenticated && IsConnected;

    /// <summary>
    /// Configura el host del servidor al que se conectará
    /// </summary>
    public void SetServerHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Server host cannot be empty", nameof(host));
            
        // Si ya está conectado y se cambia el host, desconectar
        if (IsConnected && _serverHost != host)
        {
            Disconnect();
        }
        
        _serverHost = host;
    }

    /// <summary>
    /// Obtiene el host del servidor actual
    /// </summary>
    public string GetServerHost() => _serverHost;

    /// <summary>
    /// Conecta al servidor TCP
    /// </summary>
    public async Task<bool> ConnectAsync()
    {
        await _connectionLock.WaitAsync();
        try
        {
            return await ConnectInternalAsync();
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <summary>
    /// Conecta al servidor TCP (versión interna sin lock)
    /// </summary>
    private async Task<bool> ConnectInternalAsync()
    {
        try
        {
            if (IsConnected)
                return true;

            Disconnect();

            Console.WriteLine($"Connecting to server: {_serverHost}:{ServerPort}");
            
            _client = new TcpClient();
            await _client.ConnectAsync(_serverHost, ServerPort);

            _stream = _client.GetStream();
            _reader = new StreamReader(_stream, Encoding.UTF8);
            _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };

            Console.WriteLine($"Successfully connected to {_serverHost}:{ServerPort}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to server {_serverHost}:{ServerPort}: {ex.Message}");
            Disconnect();
            return false;
        }
    }

    /// <summary>
    /// Envía un comando al servidor y espera respuesta
    /// </summary>
    public async Task<string?> SendCommandAsync(string command)
    {
        await _connectionLock.WaitAsync();
        try
        {
            // Conectar si no está conectado (sin tomar el lock nuevamente)
            if (!IsConnected)
            {
                var connected = await ConnectInternalAsync();
                if (!connected)
                    return null;
            }

            await _writer!.WriteLineAsync(command);
            var response = await _reader!.ReadLineAsync();
            
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending command: {ex.Message}");
            Disconnect();
            return null;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <summary>
    /// Envía un comando con datos binarios (para upload de videos)
    /// </summary>
    public async Task<string?> SendCommandWithDataAsync(string command, byte[] data)
    {
        await _connectionLock.WaitAsync();
        try
        {
            // Conectar si no está conectado (sin tomar el lock nuevamente)
            if (!IsConnected)
            {
                var connected = await ConnectInternalAsync();
                if (!connected)
                    return null;
            }

            // Enviar comando
            await _writer!.WriteLineAsync(command);
            
            // Enviar datos binarios
            await _stream!.WriteAsync(data, 0, data.Length);
            await _stream.FlushAsync();
            
            // Leer respuesta
            var response = await _reader!.ReadLineAsync();
            
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending command with data: {ex.Message}");
            Disconnect();
            return null;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <summary>
    /// Marca que el usuario se autenticó exitosamente
    /// </summary>
    public void SetAuthenticated(bool authenticated)
    {
        _isAuthenticated = authenticated;
    }

    /// <summary>
    /// Desconecta del servidor
    /// </summary>
    public void Disconnect()
    {
        _isAuthenticated = false;
        
        try
        {
            _writer?.Close();
            _reader?.Close();
            _stream?.Close();
            _client?.Close();
        }
        catch { }
        finally
        {
            _writer = null;
            _reader = null;
            _stream = null;
            _client = null;
        }
    }

    public void Dispose()
    {
        Disconnect();
        _connectionLock?.Dispose();
    }
}
