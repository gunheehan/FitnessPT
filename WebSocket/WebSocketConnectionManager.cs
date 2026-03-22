using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace FitnessPT.WebSocket;

/// <summary>
/// 모든 WebSocket 연결을 관리합니다. Singleton으로 등록합니다.
/// </summary>
public class WebSocketConnectionManager
{
    private readonly ConcurrentDictionary<string, WebSocketConnection> _connections = new();
    private readonly int _maxConnections;

    public WebSocketConnectionManager(int maxConnections = 1000)
    {
        _maxConnections = maxConnections;
    }

    public int ConnectionCount => _connections.Count;

    /// <summary>
    /// 연결을 추가합니다. 최대 연결 수를 초과하면 null을 반환합니다.
    /// </summary>
    public WebSocketConnection? AddConnection(System.Net.WebSockets.WebSocket socket)
    {
        if (_connections.Count >= _maxConnections) return null;

        var connection = new WebSocketConnection { Socket = socket };
        _connections[connection.ConnectionId] = connection;
        return connection;
    }

    public void RemoveConnection(string connectionId)
    {
        if (_connections.TryRemove(connectionId, out var conn))
            conn.Dispose();
    }

    public WebSocketConnection? GetConnection(string connectionId)
        => _connections.GetValueOrDefault(connectionId);

    public IEnumerable<WebSocketConnection> GetAllConnections()
        => _connections.Values;

    public IEnumerable<WebSocketConnection> GetConnectionsByUser(string userId)
        => _connections.Values.Where(c => c.UserId == userId);

    /// <summary>특정 연결에 메시지를 전송합니다.</summary>
    public Task SendAsync(string connectionId, string message, CancellationToken ct = default)
    {
        // Send는 WebSocketConnection 내부의 잠금으로 직렬화됨
        if (_connections.TryGetValue(connectionId, out var connection))
            return connection.SendAsync(message, ct);

        return Task.CompletedTask;
    }

    /// <summary>열려있는 모든 연결에 메시지를 브로드캐스트합니다.</summary>
    public Task BroadcastAsync(string message, string? excludeConnectionId = null, CancellationToken ct = default)
    {
        var tasks = _connections.Values
            .Where(c => c.IsOpen && c.ConnectionId != excludeConnectionId)
            .Select(c => c.SendAsync(message, ct));

        return Task.WhenAll(tasks);
    }

    /// <summary>특정 사용자의 모든 연결에 메시지를 전송합니다.</summary>
    public Task SendToUserAsync(string userId, string message, CancellationToken ct = default)
    {
        var tasks = GetConnectionsByUser(userId)
            .Where(c => c.IsOpen)
            .Select(c => c.SendAsync(message, ct));

        return Task.WhenAll(tasks);
    }
}
