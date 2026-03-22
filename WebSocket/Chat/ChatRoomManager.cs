using System.Collections.Concurrent;

namespace FitnessPT.WebSocket.Chat;

/// <summary>
/// 채팅 룸 멤버십을 관리합니다. Singleton으로 등록합니다.
/// ConcurrentDictionary + 룸별 전용 lock 객체로 스레드 안전성을 보장합니다.
/// </summary>
public class ChatRoomManager
{
    // roomId → (connectionId 집합, lock 객체)
    // AddOrUpdate 콜백 재호출 문제를 피하기 위해 값 자체에 lock을 포함
    private readonly ConcurrentDictionary<string, RoomEntry> _rooms = new();
    // connectionId → roomId
    private readonly ConcurrentDictionary<string, string> _connectionRoom = new();

    public void JoinRoom(string roomId, string connectionId)
    {
        var entry = _rooms.GetOrAdd(roomId, _ => new RoomEntry());
        lock (entry)
        {
            entry.Members.Add(connectionId);
        }
        _connectionRoom[connectionId] = roomId;
    }

    public string? LeaveRoom(string connectionId)
    {
        if (!_connectionRoom.TryRemove(connectionId, out var roomId)) return null;

        if (_rooms.TryGetValue(roomId, out var entry))
        {
            lock (entry)
            {
                entry.Members.Remove(connectionId);
            }
        }

        return roomId;
    }

    public IReadOnlyList<string> GetRoomMembers(string roomId)
    {
        if (_rooms.TryGetValue(roomId, out var entry))
        {
            lock (entry)
            {
                return entry.Members.ToList();
            }
        }
        return [];
    }

    public string? GetUserRoom(string connectionId)
        => _connectionRoom.GetValueOrDefault(connectionId);

    private sealed class RoomEntry
    {
        public HashSet<string> Members { get; } = [];
    }
}
