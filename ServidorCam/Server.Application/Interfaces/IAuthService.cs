using Server.Application.DTOs;

namespace Server.Application.Interfaces;

public interface IAuthService
{
    Task<int?> LoginAsync(LoginRequest request);
    Task<bool> RegisterAsync(RegisterRequest request);
}