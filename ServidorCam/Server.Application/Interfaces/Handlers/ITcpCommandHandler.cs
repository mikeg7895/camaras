namespace Server.Application.Interfaces.Handlers;

public interface ITcpCommandHandler
{
    string Command { get; }
    Task<string> HandleAsync(string[] args);
}
