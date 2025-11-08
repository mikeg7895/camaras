using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CameraClient.Desktop.Models;

namespace CameraClient.Desktop.Services;

public class AuthService : IDisposable
{
    private const string ServerHost = "localhost";
    private const int ServerPort = 5000;
    private TcpClient? _client;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            await EnsureConnectedAsync();

            // Formato: LOGIN|email|password
            var command = $"LOGIN|{request.Email}|{request.Password}";
            await _writer!.WriteLineAsync(command);

            var response = await _reader!.ReadLineAsync();
            
            if (string.IsNullOrEmpty(response))
            {
                return new AuthResponse 
                { 
                    Success = false, 
                    Message = "No response from server" 
                };
            }

            return ParseLoginResponse(response, request.Email);
        }
        catch (Exception ex)
        {
            Disconnect();
            return new AuthResponse 
            { 
                Success = false, 
                Message = $"Connection error: {ex.Message}" 
            };
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            await EnsureConnectedAsync();

            // Formato: REGISTER|username|email|password
            var command = $"REGISTER|{request.Username}|{request.Email}|{request.Password}";
            await _writer!.WriteLineAsync(command);

            var response = await _reader!.ReadLineAsync();
            
            if (string.IsNullOrEmpty(response))
            {
                return new AuthResponse 
                { 
                    Success = false, 
                    Message = "No response from server" 
                };
            }

            return ParseRegisterResponse(response);
        }
        catch (Exception ex)
        {
            Disconnect();
            return new AuthResponse 
            { 
                Success = false, 
                Message = $"Connection error: {ex.Message}" 
            };
        }
    }

    private async Task EnsureConnectedAsync()
    {
        if (_client?.Connected == true)
            return;

        _client = new TcpClient();
        await _client.ConnectAsync(ServerHost, ServerPort);

        var stream = _client.GetStream();
        _reader = new StreamReader(stream, Encoding.UTF8);
        _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
    }

    private AuthResponse ParseLoginResponse(string response, string email)
    {
        // Formato de respuesta: SUCCESS|userId o ERROR|mensaje
        var parts = response.Split('|', 2);
        
        if (parts.Length < 2)
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Invalid server response format" 
            };
        }

        var status = parts[0];
        var data = parts[1];

        if (status == "SUCCESS")
        {
            // El servidor devuelve el userId como string
            if (int.TryParse(data, out var userId))
            {
                return new AuthResponse
                {
                    Success = true,
                    Message = "Login successful",
                    Token = userId.ToString(), // Usamos el userId como token por ahora
                    User = new User
                    {
                        Id = userId,
                        Email = email
                    }
                };
            }
            
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid user ID in response"
            };
        }

        return new AuthResponse
        {
            Success = false,
            Message = data
        };
    }

    private AuthResponse ParseRegisterResponse(string response)
    {
        // Formato de respuesta: SUCCESS|mensaje o ERROR|mensaje
        var parts = response.Split('|', 2);
        
        if (parts.Length < 2)
        {
            return new AuthResponse 
            { 
                Success = false, 
                Message = "Invalid server response format" 
            };
        }

        var status = parts[0];
        var message = parts[1];

        return new AuthResponse
        {
            Success = status == "SUCCESS",
            Message = message
        };
    }

    private void Disconnect()
    {
        try
        {
            _reader?.Dispose();
            _writer?.Dispose();
            _client?.Close();
        }
        catch { }
        finally
        {
            _reader = null;
            _writer = null;
            _client = null;
        }
    }

    public void Dispose()
    {
        Disconnect();
        GC.SuppressFinalize(this);
    }
}
