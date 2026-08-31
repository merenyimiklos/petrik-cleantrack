namespace CleanTrack.Api.Models;

public class Terminal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DeviceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ApiKeyHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? LastSeenAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<AttendanceEvent> AttendanceEvents { get; set; } = [];
}
