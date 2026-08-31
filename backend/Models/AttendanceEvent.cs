namespace CleanTrack.Api.Models;

public class AttendanceEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public Guid? TerminalId { get; set; }
    public Terminal? Terminal { get; set; }
    public AttendanceEventType Type { get; set; }
    public AttendanceSource Source { get; set; }
    public Guid? ExternalEventId { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }
    public Guid? CreatedByUserId { get; set; }
}
