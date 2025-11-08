namespace Server.Application.Interfaces;

public interface ITcpRequestHandler
{
    Task<string> HandleRequestAsync(string request);
}