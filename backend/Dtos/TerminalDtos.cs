namespace CleanTrack.Api.Dtos;

public record CreateTerminalRequest(string DeviceId, string Name);
public record CreateTerminalResponse(Guid Id, string DeviceId, string Name, string ApiKey);
