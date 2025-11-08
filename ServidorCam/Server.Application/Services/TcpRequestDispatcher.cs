using Server.Application.Interfaces;
using Server.Application.Interfaces.Handlers;

namespace Server.Application.Services;

public class TcpRequestDispatcher(IEnumerable<ITcpCommandHandler> handlers) : ITcpRequestHandler
{
    private readonly Dictionary<string, ITcpCommandHandler> _handlers = handlers.ToDictionary(h => h.Command);

    public async Task<string> HandleRequestAsync(string request)
    {
        var parts = request.Trim().Split('|');
        if (parts.Length == 0) return await Task.FromResult("ERROR|invalid request");
        var cmd = parts[0].ToUpperInvariant();

        if (_handlers.TryGetValue(cmd, out var handler))
        {
            return await handler.HandleAsync(parts);
        }

        return await Task.FromResult("ERROR|unknown command");
    }
}