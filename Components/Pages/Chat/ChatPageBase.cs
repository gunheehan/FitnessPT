using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text.Json;
using FitnessPT.WebSocket.Chat;

namespace FitnessPT.Components.Pages.Chat;

public class ChatPageBase : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;

    protected string UserName { get; set; } = string.Empty;
    protected string RoomId { get; set; } = "general";
    protected string InputText { get; set; } = string.Empty;
    protected string? ConnectionId { get; private set; }
    protected bool IsConnected { get; private set; }
    protected List<ChatDisplayMessage> Messages { get; } = [];

    private DotNetObjectReference<ChatPageBase>? _dotnetRef;

    // ──────────────────────────────────────────────
    // 연결 / 해제
    // ──────────────────────────────────────────────

    protected async Task ConnectAsync()
    {
        if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(RoomId)) return;

        _dotnetRef?.Dispose();
        _dotnetRef = DotNetObjectReference.Create(this);

        var wsUrl = Nav.BaseUri
            .Replace("https://", "wss://")
            .Replace("http://", "ws://")
            .TrimEnd('/') + "/ws/chat";

        await JS.InvokeVoidAsync("ChatWs.connect", _dotnetRef, wsUrl);
    }

    protected async Task DisconnectAsync()
    {
        await SendWsAsync(new ChatMessage { Type = "leave" });
        await JS.InvokeVoidAsync("ChatWs.disconnect");
        IsConnected = false;
        ConnectionId = null;
        Messages.Clear();
        StateHasChanged();
    }

    // ──────────────────────────────────────────────
    // 메시지 수신 (JS → C#)
    // ──────────────────────────────────────────────

    [JSInvokable]
    public async Task OnMessageReceived(string json)
    {
        var msg = JsonSerializer.Deserialize<ChatMessage>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (msg is null) return;

        switch (msg.Type)
        {
            case "connected":
                // 서버가 connectionId를 Content로 전달
                ConnectionId = msg.Content;
                IsConnected = true;
                // 룸 입장 요청
                await SendWsAsync(new ChatMessage
                {
                    Type = "join",
                    RoomId = RoomId,
                    UserName = UserName
                });
                break;

            case "joined":
            case "left":
            case "message":
                Messages.Add(new ChatDisplayMessage(msg));
                await ScrollToBottomAsync();
                break;

            case "ping":
                // 서버 ping에 pong 응답 (LastActivity 갱신용)
                await SendWsAsync(new ChatMessage { Type = "pong" });
                return; // StateHasChanged 불필요

            case "pong":
                return; // 무시

            case "disconnected":
                IsConnected = false;
                ConnectionId = null;
                break;

            case "error":
                Messages.Add(new ChatDisplayMessage(new ChatMessage
                {
                    Type = "system",
                    Content = $"[오류] {msg.Content}"
                }));
                break;
        }

        await InvokeAsync(StateHasChanged);
    }

    // ──────────────────────────────────────────────
    // 메시지 전송
    // ──────────────────────────────────────────────

    protected async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(InputText)) return;

        await SendWsAsync(new ChatMessage
        {
            Type = "message",
            UserName = UserName,
            Content = InputText
        });

        InputText = string.Empty;
    }

    protected async Task OnKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
            await SendMessageAsync();
    }

    private Task SendWsAsync(ChatMessage msg)
    {
        var json = JsonSerializer.Serialize(msg);
        return JS.InvokeVoidAsync("ChatWs.send", json).AsTask();
    }

    private Task ScrollToBottomAsync()
        => JS.InvokeVoidAsync("eval",
            "setTimeout(() => { var el = document.getElementById('chatMessages'); if(el) el.scrollTop = el.scrollHeight; }, 50)").AsTask();

    // ──────────────────────────────────────────────
    // Dispose
    // ──────────────────────────────────────────────

    public async ValueTask DisposeAsync()
    {
        if (IsConnected)
            await DisconnectAsync();

        _dotnetRef?.Dispose();
    }
}

/// <summary>화면 표시용 메시지 래퍼 (시간 포맷 등 UI 전용 필드 포함)</summary>
public class ChatDisplayMessage
{
    public string Type { get; }
    public string? UserId { get; }
    public string? UserName { get; }
    public string? Content { get; }
    public string DisplayTime { get; }

    public ChatDisplayMessage(ChatMessage msg)
    {
        Type = msg.Type;
        UserId = msg.UserId;
        UserName = msg.UserName;
        Content = msg.Content;

        if (DateTime.TryParse(msg.Timestamp, out var dt))
            DisplayTime = dt.ToLocalTime().ToString("HH:mm");
        else
            DisplayTime = string.Empty;
    }
}
