namespace HamsterWoods.Hubs;

public interface IConnectionProvider
{
    void Add(string clientId, string connectionId);
    string? Remove(string connectionId);
    ConnectionInfo? GetConnectionByClientId(string clientId);
    ConnectionInfo? GetConnectionByConnectionId(string connectionId);
}