namespace CleanTrack.Api.Dtos;

public record EmployeeRequest(string EmployeeCode, string FullName, int DailyTargetMinutes = 480, bool IsActive = true);
public record AssignRfidRequest(string RfidUid);
