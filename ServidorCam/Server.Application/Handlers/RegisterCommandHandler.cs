using Server.Application.DTOs;
using Server.Application.Interfaces;
using Server.Application.Interfaces.Handlers;
using Server.Core.Domain.Entities;

namespace Server.Application.Handlers;

public class RegisterCommandHandler(IUserService userService, IAuthService authService) : ITcpCommandHandler
{
    private readonly IUserService _userService = userService;
    private readonly IAuthService _authService = authService;

    public string Command => "REGISTER";

    public async Task<string> HandleAsync(string[] parameters)
    {
        // Formato esperado: REGISTER|username|email|password
        if (parameters.Length < 4)
        {
            return "ERROR|Invalid parameters for REGISTER command.";
        }

        var registerRequest = new RegisterRequest(parameters[1], parameters[2], parameters[3]);

        var existingUsers = await _userService.GetAllAsync();
        if (existingUsers.Any(u => u.Username == registerRequest.Username))
        {
            return "ERROR|Username already exists.";
        }

        var value = await _authService.RegisterAsync(registerRequest);
        if (!value) return "ERROR|User registration failed.";

        return "SUCCESS|User registered successfully. Awaiting approval.";
    }
}