using System.Net.Sockets;

namespace Server.Application.Interfaces.Handlers;

public interface ITcpCommandHandler
{
    string Command { get; }
    Task<string> HandleAsync(string[] args, NetworkStream? networkStream = null);
}
