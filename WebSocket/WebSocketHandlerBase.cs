namespace FitnessPT.WebSocket;

/// <summary>
/// WebSocket 핸들러의 추상 기반 클래스.
/// 연결 관리(ConnectionManager)를 주입받고 공통 로직을 제공합니다.
/// 채팅, 알림 등 구체적인 핸들러는 이 클래스를 상속합니다.
/// </summary>
public abstract class WebSocketHandlerBase : IWebSocketHandler
{
    protected readonly WebSocketConnectionManager ConnectionManager;

    protected WebSocketHandlerBase(WebSocketConnectionManager connectionManager)
    {
        ConnectionManager = connectionManager;
    }

    public virtual Task OnConnectedAsync(WebSocketConnection connection)
    {
        // 기본 동작: 아무 작업 없음. 필요 시 오버라이드.
        return Task.CompletedTask;
    }

    public virtual Task OnDisconnectedAsync(WebSocketConnection connection, Exception? exception)
    {
        return Task.CompletedTask;
    }

    public abstract Task ReceiveAsync(WebSocketConnection connection, string message);
}
