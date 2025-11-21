using System;
using System.Text.Json;
using System.Threading.Tasks;
using CameraClient.Desktop.Models;

namespace CameraClient.Desktop.Services;

public class AuthService
{
    private readonly TcpConnectionService _connectionService;

    public AuthService(TcpConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            // Formato: LOGIN|email|password
            var command = $"LOGIN|{request.Email}|{request.Password}";
            var response = await _connectionService.SendCommandAsync(command);
            
            if (string.IsNullOrEmpty(response))
            {
                return new AuthResponse 
                { 
                    Success = false, 
                    Message = "No response from server" 
                };
            }

            var authResponse = ParseLoginResponse(response, request.Email);
            
            // Marcar como autenticado si fue exitoso
            if (authResponse.Success)
            {
                _connectionService.SetAuthenticated(true);
            }
            
            return authResponse;
        }
        catch (Exception ex)
        {
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
            // Formato: REGISTER|username|email|password
            var command = $"REGISTER|{request.Username}|{request.Email}|{request.Password}";
            var response = await _connectionService.SendCommandAsync(command);
            
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
            return new AuthResponse 
            { 
                Success = false, 
                Message = $"Connection error: {ex.Message}" 
            };
        }
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

    public void Logout()
    {
        _connectionService.SetAuthenticated(false);
        _connectionService.Disconnect();
    }
}
