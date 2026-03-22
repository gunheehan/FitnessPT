using System.Collections.Concurrent;
using System.Text.Json;

namespace FitnessPT.WebSocket.Chat;

/// <summary>
/// /ws/chat 경로의 WebSocket 채팅 핸들러.
/// </summary>
public class ChatWebSocketHandler : WebSocketHandlerBase
{
    private readonly ChatRoomManager _roomManager;
    private readonly ILogger<ChatWebSocketHandler> _logger;

    // Rate Limiting: connectionId → 초당 메시지 카운터
    private readonly ConcurrentDictionary<string, (int count, DateTime windowStart)> _rateLimit = new();

    private const int MaxContentLength = 1000;         // 메시지 최대 길이
    private const int MaxRoomIdLength = 50;            // roomId 최대 길이
    private const int MaxUserNameLength = 30;          // 이름 최대 길이
    private const int RateLimitPerSecond = 5;          // 초당 최대 메시지 수

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ChatWebSocketHandler(
        WebSocketConnectionManager connectionManager,
        ChatRoomManager roomManager,
        ILogger<ChatWebSocketHandler> logger) : base(connectionManager)
    {
        _roomManager = roomManager;
        _logger = logger;
    }

    public override async Task OnConnectedAsync(WebSocketConnection connection)
    {
        _logger.LogInformation("Chat connected: {ConnectionId}", connection.ConnectionId);

        await SendAsync(connection.ConnectionId, new ChatMessage
        {
            Type = "connected",
            Content = connection.ConnectionId
        });
    }

    public override async Task OnDisconnectedAsync(WebSocketConnection connection, Exception? exception)
    {
        _rateLimit.TryRemove(connection.ConnectionId, out _);

        var roomId = _roomManager.LeaveRoom(connection.ConnectionId);
        if (roomId is not null)
        {
            await BroadcastToRoomAsync(roomId, new ChatMessage
            {
                Type = "left",
                RoomId = roomId,
                UserId = connection.UserId,
                UserName = connection.UserName
            }, exclude: connection.ConnectionId);
        }

        await base.OnDisconnectedAsync(connection, exception);
    }

    public override async Task ReceiveAsync(WebSocketConnection connection, string message)
    {
        // ── Rate Limiting ──────────────────────────────────
        if (IsRateLimited(connection.ConnectionId))
        {
            await SendErrorAsync(connection.ConnectionId, "Too many messages. Slow down.");
            return;
        }

        ChatMessage? msg;
        try
        {
            msg = JsonSerializer.Deserialize<ChatMessage>(message, JsonOptions);
        }
        catch
        {
            await SendErrorAsync(connection.ConnectionId, "Invalid JSON");
            return;
        }

        if (msg is null) return;

        switch (msg.Type)
        {
            case "join":    await HandleJoinAsync(connection, msg);    break;
            case "message": await HandleMessageAsync(connection, msg); break;
            case "leave":   await HandleLeaveAsync(connection);        break;
            default:
                await SendErrorAsync(connection.ConnectionId, $"Unknown type: {msg.Type}");
                break;
        }
    }

    // ──────────────────────────────────────────────
    // 핸들러
    // ──────────────────────────────────────────────

    private async Task HandleJoinAsync(WebSocketConnection connection, ChatMessage msg)
    {
        // ── 입력 검증 ──────────────────────────────────────
        if (string.IsNullOrWhiteSpace(msg.RoomId) || msg.RoomId.Length > MaxRoomIdLength)
        {
            await SendErrorAsync(connection.ConnectionId, "Invalid roomId");
            return;
        }
        if (string.IsNullOrWhiteSpace(msg.UserName) || msg.UserName.Length > MaxUserNameLength)
        {
            await SendErrorAsync(connection.ConnectionId, "Invalid userName");
            return;
        }

        // 이미 다른 룸에 있으면 먼저 나가기
        if (_roomManager.GetUserRoom(connection.ConnectionId) is not null)
            await HandleLeaveAsync(connection);

        // ── 보안: UserId/UserName은 Join 시 한 번만 서버에 저장 ──
        // 이후 메시지에서 클라이언트가 보내는 값은 무시하고 여기 저장된 값 사용
        connection.UserId = msg.UserId ?? connection.ConnectionId;
        connection.UserName = msg.UserName.Trim();

        _roomManager.JoinRoom(msg.RoomId, connection.ConnectionId);

        await BroadcastToRoomAsync(msg.RoomId, new ChatMessage
        {
            Type = "joined",
            RoomId = msg.RoomId,
            UserId = connection.UserId,
            UserName = connection.UserName
        });
    }

    private async Task HandleMessageAsync(WebSocketConnection connection, ChatMessage msg)
    {
        var roomId = _roomManager.GetUserRoom(connection.ConnectionId);
        if (roomId is null)
        {
            await SendErrorAsync(connection.ConnectionId, "Join a room first");
            return;
        }

        // ── 입력 검증 ──────────────────────────────────────
        if (string.IsNullOrWhiteSpace(msg.Content)) return;
        if (msg.Content.Length > MaxContentLength)
        {
            await SendErrorAsync(connection.ConnectionId, $"Message too long (max {MaxContentLength})");
            return;
        }

        await BroadcastToRoomAsync(roomId, new ChatMessage
        {
            Type = "message",
            RoomId = roomId,
            UserId = connection.UserId,
            UserName = connection.UserName,  // 클라이언트 값 무시, 서버 저장값 사용
            Content = msg.Content.Trim()
        });
    }

    private async Task HandleLeaveAsync(WebSocketConnection connection)
    {
        var roomId = _roomManager.LeaveRoom(connection.ConnectionId);
        if (roomId is null) return;

        await BroadcastToRoomAsync(roomId, new ChatMessage
        {
            Type = "left",
            RoomId = roomId,
            UserId = connection.UserId,
            UserName = connection.UserName
        }, exclude: connection.ConnectionId);
    }

    // ──────────────────────────────────────────────
    // Rate Limiting
    // ──────────────────────────────────────────────

    private bool IsRateLimited(string connectionId)
    {
        var now = DateTime.UtcNow;
        var state = _rateLimit.GetOrAdd(connectionId, _ => (0, now));

        int newCount;
        DateTime windowStart;

        if ((now - state.windowStart).TotalSeconds >= 1)
        {
            newCount = 1;
            windowStart = now;
        }
        else
        {
            newCount = state.count + 1;
            windowStart = state.windowStart;
        }

        _rateLimit[connectionId] = (newCount, windowStart);
        return newCount > RateLimitPerSecond;
    }

    // ──────────────────────────────────────────────
    // 유틸
    // ──────────────────────────────────────────────

    private async Task BroadcastToRoomAsync(string roomId, ChatMessage msg, string? exclude = null)
    {
        var json = JsonSerializer.Serialize(msg);
        var tasks = _roomManager.GetRoomMembers(roomId)
            .Where(id => id != exclude)
            .Select(id => ConnectionManager.SendAsync(id, json));

        await Task.WhenAll(tasks);
    }

    private Task SendAsync(string connectionId, ChatMessage msg)
        => ConnectionManager.SendAsync(connectionId, JsonSerializer.Serialize(msg));

    private Task SendErrorAsync(string connectionId, string error)
        => SendAsync(connectionId, new ChatMessage { Type = "error", Content = error });
}

