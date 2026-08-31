using CleanTrack.Api.Models;

namespace CleanTrack.Api.Dtos;

public record TerminalScanRequest(Guid EventId, string RfidUid, DateTimeOffset? OccurredAt);
public record TerminalScanResponse(bool Success, string EmployeeName, AttendanceEventType Action, DateTime OccurredAtUtc, int? WorkedMinutesToday, bool Duplicate);
public record ManualAttendanceRequest(Guid EmployeeId, AttendanceEventType Type, DateTimeOffset OccurredAt, string? Note);
