using System.Net.Sockets;
using System.Text.Json;
using Server.Application.Interfaces;
using Server.Application.Interfaces.Handlers;

namespace Server.Application.Handlers;

public class UserCommandHandler(IUserService userService) : ITcpCommandHandler
{
    private readonly IUserService _userService = userService;
    public string Command => "USER";

    public async Task<string> HandleAsync(string[] args, NetworkStream? networkStream = null)
    {
        // Formato esperado: 
        // USER|GET|true/false -> Obtener usuarios aprobados/pendientes
        // USER|PUT|id         -> Aprobar usuario por ID
        
        if (args.Length < 3)
            return "ERROR|Invalid format. Usage: USER|GET|true/false or USER|PUT|id";

        var action = args[1].ToUpperInvariant();

        return action switch
        {
            "GET" => await HandleGetUsers(args[2]),
            "PUT" => await HandleApproveUser(args[2]),
            _ => "ERROR|Unknown action. Supported: GET, PUT",
        };
    }

    private async Task<string> HandleGetUsers(string approvedParam)
    {
        if (!bool.TryParse(approvedParam, out var approved))
            return "ERROR|Invalid value. Use true or false";

        try
        {
            var users = await _userService.GetUsersWithApproved(approved);

            if (users == null || !users.Any())
                return "SUCCESS|[]";

            var usersData = users.Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.Approved,
                u.LastLogin
            });

            var json = JsonSerializer.Serialize(usersData);
            return $"SUCCESS|{json}";
        }
        catch (Exception ex)
        {
            return $"ERROR|{ex.Message}";
        }
    }

    private async Task<string> HandleApproveUser(string idParam)
    {
        if (!int.TryParse(idParam, out var userId))
            return "ERROR|Invalid user ID";

        try
        {
            var user = await _userService.ApproveUser(userId);

            if (user == null)
                return "ERROR|User not found";

            var userData = new
            {
                user.Id,
                user.Username,
                user.Email,
                user.Approved,
                user.LastLogin
            };

            var json = JsonSerializer.Serialize(userData);
            return $"SUCCESS|{json}";
        }
        catch (Exception ex)
        {
            return $"ERROR|{ex.Message}";
        }
    }
}