using System.Net.WebSockets;
using System.Text;

namespace FitnessPT.WebSocket;

/// <summary>
/// 단일 WebSocket 연결을 나타냅니다.
/// SendLock: WebSocket은 동시에 Send가 하나만 허용되므로 소켓당 SemaphoreSlim으로 직렬화합니다.
/// </summary>
public class WebSocketConnection : IDisposable
{
    public string ConnectionId { get; } = Guid.NewGuid().ToString();
    public System.Net.WebSockets.WebSocket Socket { get; init; } = null!;
    public string? UserId { get; set; }
    public string? UserName { get; set; }  // Join 시 서버에 저장 → 이후 메시지에서 클라이언트 값 무시
    public DateTime ConnectedAt { get; } = DateTime.UtcNow;
    public DateTime LastActivity { get; private set; } = DateTime.UtcNow;

    public bool IsOpen => Socket.State == WebSocketState.Open;

    // 소켓당 Send 직렬화 잠금 (PingLoop와 Broadcast 동시 Send 충돌 방지)
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    public void UpdateLastActivity() => LastActivity = DateTime.UtcNow;

    /// <summary>
    /// 이 연결에 메시지를 전송합니다. 내부적으로 Send를 직렬화합니다.
    /// </summary>
    public async Task SendAsync(string message, CancellationToken ct = default)
    {
        if (!IsOpen) return;

        await _sendLock.WaitAsync(ct);
        try
        {
            if (!IsOpen) return;
            var buffer = Encoding.UTF8.GetBytes(message);
            await Socket.SendAsync(buffer, WebSocketMessageType.Text, true, ct);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    /// <summary>WebSocket Ping 프레임을 전송합니다.</summary>
    public async Task SendPingAsync(CancellationToken ct = default)
    {
        if (!IsOpen) return;

        await _sendLock.WaitAsync(ct);
        try
        {
            if (!IsOpen) return;
            // Application-level ping: JSON 형식으로 전송
            // (ASP.NET Core WebSocket API는 Ping opcode를 직접 노출하지 않음)
            var buffer = Encoding.UTF8.GetBytes("{\"type\":\"ping\"}");
            await Socket.SendAsync(buffer, WebSocketMessageType.Text, true, ct);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public void Dispose() => _sendLock.Dispose();
}
