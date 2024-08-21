using System.Collections.Concurrent;
using Volo.Abp.DependencyInjection;

namespace HamsterWoods.Hubs;

public class ConnectionProvider : IConnectionProvider, ISingletonDependency
{
    private readonly ConcurrentDictionary<string, ConnectionInfo> _connections = new();
    private readonly ConcurrentDictionary<string, string> _connectionIds = new();

    public void Add(string clientId, string connectionId)
    {
        _connections[clientId] = new ConnectionInfo
        {
            ConnectionId = connectionId,
            ClientId = clientId,
        };
        _connectionIds[connectionId] = clientId;
    }

    public string? Remove(string connectionId)
    {
        if (!_connectionIds.TryGetValue(connectionId, out var clientId))
        {
            return clientId;
        }

        _connections.TryRemove(clientId, out _);
        _connectionIds.TryRemove(connectionId, out _);

        return clientId;
    }

    public ConnectionInfo? GetConnectionByClientId(string clientId)
    {
        return _connections.TryGetValue(clientId, out var connection) ? connection : null;
    }

    public ConnectionInfo? GetConnectionByConnectionId(string connectionId)
    {
        return _connectionIds.TryGetValue(connectionId, out var clientId) ? GetConnectionByClientId(clientId) : null;
    }
}