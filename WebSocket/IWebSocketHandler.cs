namespace FitnessPT.WebSocket;

/// <summary>
/// WebSocket 이벤트를 처리하는 핸들러 인터페이스.
/// 채팅, 알림 등 각 기능별 핸들러는 이 인터페이스를 구현합니다.
/// </summary>
public interface IWebSocketHandler
{
    /// <summary>클라이언트가 연결되었을 때 호출됩니다.</summary>
    Task OnConnectedAsync(WebSocketConnection connection);

    /// <summary>클라이언트 연결이 끊겼을 때 호출됩니다.</summary>
    Task OnDisconnectedAsync(WebSocketConnection connection, Exception? exception);

    /// <summary>클라이언트로부터 메시지를 수신했을 때 호출됩니다.</summary>
    Task ReceiveAsync(WebSocketConnection connection, string message);
}
