using Server.Application.DTOs;
using Server.Application.Interfaces;
using Server.Application.Interfaces.Handlers;

namespace Server.Application.Handlers;

public class LoginCommandHandler(IAuthService authService) : ITcpCommandHandler
{
    private readonly IAuthService _authService = authService;
    public string Command => "LOGIN";

    public async Task<string> HandleAsync(string[] args)
    {
        if (args.Length < 3) return "ERROR|missing username or password";

        var loginRequest = new LoginRequest(args[1], args[2]);

        var result = await _authService.LoginAsync(loginRequest);
        return result is not null ? $"SUCCESS|{result}" : "ERROR|invalid credentials or not approved";
    }
}