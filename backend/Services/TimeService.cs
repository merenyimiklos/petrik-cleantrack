namespace CleanTrack.Api.Services;

public class TimeService(IConfiguration configuration)
{
    private readonly TimeZoneInfo _timeZone = TimeZoneInfo.FindSystemTimeZoneById(configuration["TimeZone"] ?? "Europe/Budapest");

    public DateTime UtcNow => DateTime.UtcNow;

    public (DateTime fromUtc, DateTime toUtc) GetLocalDayUtcRange(DateTime utcReference)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcReference, DateTimeKind.Utc), _timeZone);
        var localStart = DateTime.SpecifyKind(local.Date, DateTimeKind.Unspecified);
        var localEnd = localStart.AddDays(1);
        return (TimeZoneInfo.ConvertTimeToUtc(localStart, _timeZone), TimeZoneInfo.ConvertTimeToUtc(localEnd, _timeZone));
    }
}
