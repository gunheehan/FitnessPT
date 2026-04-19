using System.Buffers;
using System.Net.WebSockets;
using System.Text;

namespace FitnessPT.WebSocket;

/// <summary>
/// HTTP 요청을 WebSocket으로 업그레이드하고, 수신 루프를 관리하는 미들웨어.
/// </summary>
public class WebSocketMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<WebSocketMiddleware> _logger;
    private readonly WebSocketMiddlewareOptions _options;

    private readonly Dictionary<string, Type> _handlerMap = new();

    public WebSocketMiddleware(RequestDelegate next, ILogger<WebSocketMiddleware> logger,
        WebSocketMiddlewareOptions? options = null)
    {
        _next = next;
        _logger = logger;
        _options = options ?? new WebSocketMiddlewareOptions();
    }

    public void MapHandler(string path, Type handlerType)
        => _handlerMap[path] = handlerType;

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            await _next(context);
            return;
        }

        // ── 보안: Origin 검증 ──────────────────────────────
        if (_options.AllowedOrigins.Count > 0)
        {
            var origin = context.Request.Headers.Origin.ToString();
            if (!_options.AllowedOrigins.Contains(origin))
            {
                _logger.LogWarning("WebSocket rejected - invalid origin: {Origin}", origin);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
        }

        var path = context.Request.Path.Value ?? string.Empty;

        if (!_handlerMap.TryGetValue(path, out var handlerType))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var handler = serviceProvider.GetRequiredService(handlerType) as IWebSocketHandler;
        if (handler is null)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return;
        }

        var connectionManager = serviceProvider.GetRequiredService<WebSocketConnectionManager>();

        // ── 보안: 최대 연결 수 초과 거부 ────────────────────
        var socket = await context.WebSockets.AcceptWebSocketAsync();
        var connection = connectionManager.AddConnection(socket);
        if (connection is null)
        {
            _logger.LogWarning("WebSocket rejected - max connections reached ({Max})", connectionManager.ConnectionCount);
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server full", CancellationToken.None);
            return;
        }

        _logger.LogInformation("WebSocket connected: {ConnectionId} from {RemoteIp}",
            connection.ConnectionId, context.Connection.RemoteIpAddress);

        using var cts = new CancellationTokenSource();
        Task? receiveTask = null;
        Task? pingTask = null;
        Exception? disconnectException = null;

        try
        {
            await handler.OnConnectedAsync(connection);

            receiveTask = ReceiveLoopAsync(connection, handler, cts.Token);
            pingTask = PingLoopAsync(connection, cts.Token);

            // 먼저 끝난 Task를 await해서 예외를 전파받음
            var completed = await Task.WhenAny(receiveTask, pingTask);
            await completed; // 예외가 있으면 여기서 throw
        }
        catch (OperationCanceledException) { /* 정상 종료 */ }
        catch (Exception ex)
        {
            disconnectException = ex;
            _logger.LogError(ex, "WebSocket error: {ConnectionId}", connection.ConnectionId);
        }
        finally
        {
            await cts.CancelAsync();

            try
            {
                if (receiveTask is not null || pingTask is not null)
                {
                    await Task.WhenAll(receiveTask ?? Task.CompletedTask, pingTask ?? Task.CompletedTask);
                }
            }
            catch (OperationCanceledException) { /* 정상 종료 */ }
            catch (Exception ex) when (disconnectException is null)
            {
                disconnectException = ex;
                _logger.LogError(ex, "WebSocket shutdown error: {ConnectionId}", connection.ConnectionId);
            }

            try
            {
                await handler.OnDisconnectedAsync(connection, disconnectException);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WebSocket disconnect handler failed: {ConnectionId}", connection.ConnectionId);
            }

            connectionManager.RemoveConnection(connection.ConnectionId);

            if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
            {
                try { await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None); }
                catch { /* 이미 끊긴 소켓 무시 */ }
            }

            _logger.LogInformation("WebSocket disconnected: {ConnectionId}", connection.ConnectionId);
        }
    }

    // ──────────────────────────────────────────────────────────
    // 수신 루프: 멀티프레임 조합 + 메시지 크기 제한 + 타임아웃
    // ──────────────────────────────────────────────────────────

    private async Task ReceiveLoopAsync(WebSocketConnection connection, IWebSocketHandler handler,
        CancellationToken ct)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(_options.BufferSize);
        // 멀티프레임 메시지 조합용 빌더
        using var messageBuilder = new MemoryStream();

        try
        {
            while (!ct.IsCancellationRequested && connection.IsOpen)
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeoutCts.CancelAfter(_options.ReceiveTimeout);

                WebSocketReceiveResult result;
                try
                {
                    result = await connection.Socket.ReceiveAsync(buffer, timeoutCts.Token);
                }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                {
                    _logger.LogWarning("WebSocket receive timeout: {ConnectionId}", connection.ConnectionId);
                    break;
                }

                connection.UpdateLastActivity();

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                if (result.MessageType != WebSocketMessageType.Text) continue;

                // ── 보안: 메시지 누적 크기 제한 ─────────────
                if (messageBuilder.Length + result.Count > _options.MaxMessageSize)
                {
                    _logger.LogWarning("WebSocket message too large: {ConnectionId}", connection.ConnectionId);
                    messageBuilder.SetLength(0);
                    await connection.SendAsync("{\"type\":\"error\",\"content\":\"Message too large\"}", ct);
                    continue;
                }

                messageBuilder.Write(buffer, 0, result.Count);

                // 멀티프레임: EndOfMessage가 true일 때만 처리
                if (!result.EndOfMessage) continue;

                var message = Encoding.UTF8.GetString(messageBuilder.ToArray());
                messageBuilder.SetLength(0); // 다음 메시지를 위해 초기화

                // Application-level pong 응답
                if (message == "{\"type\":\"ping\"}")
                {
                    await connection.SendAsync("{\"type\":\"pong\"}", ct);
                    continue;
                }

                await handler.ReceiveAsync(connection, message);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    // ──────────────────────────────────────────────────────────
    // Ping 루프: Application-level ping으로 좀비 연결 탐지
    // ──────────────────────────────────────────────────────────

    private async Task PingLoopAsync(WebSocketConnection connection, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && connection.IsOpen)
        {
            await Task.Delay(_options.PingInterval, ct);

            if (!connection.IsOpen) break;

            if (DateTime.UtcNow - connection.LastActivity > _options.PingInterval * 2)
            {
                _logger.LogWarning("WebSocket zombie detected: {ConnectionId}", connection.ConnectionId);
                break;
            }

            try
            {
                // SemaphoreSlim으로 보호된 SendPingAsync 사용 → Broadcast와 충돌 없음
                await connection.SendPingAsync(ct);
            }
            catch
            {
                break;
            }
        }
    }
}

/// <summary>WebSocket 미들웨어 옵션</summary>
public class WebSocketMiddlewareOptions
{
    /// <summary>수신 버퍼 크기 (bytes). 기본 4KB.</summary>
    public int BufferSize { get; set; } = 4096;

    /// <summary>단일 메시지 최대 크기 (bytes). 기본 64KB.</summary>
    public int MaxMessageSize { get; set; } = 64 * 1024;

    /// <summary>이 시간 안에 수신이 없으면 연결 종료. 기본 2분.</summary>
    public TimeSpan ReceiveTimeout { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>Ping 전송 주기. 기본 30초.</summary>
    public TimeSpan PingInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>허용된 Origin 목록. 비어있으면 검증 안 함.</summary>
    public HashSet<string> AllowedOrigins { get; set; } = [];
}
