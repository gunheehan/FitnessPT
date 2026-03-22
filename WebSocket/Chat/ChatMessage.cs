using System.Text.Json.Serialization;

namespace FitnessPT.WebSocket.Chat;

/// <summary>
/// WebSocket 채팅 메시지 프로토콜.
/// Type 값: connected | join | joined | message | leave | left | error
/// </summary>
public class ChatMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("roomId")]
    public string? RoomId { get; set; }

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("userName")]
    public string? UserName { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
}
