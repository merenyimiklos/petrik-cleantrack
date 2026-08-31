namespace CleanTrack.Api.Models;

public class Employee
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? RfidUid { get; set; }
    public int DailyTargetMinutes { get; set; } = 480;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<AttendanceEvent> AttendanceEvents { get; set; } = [];
}
