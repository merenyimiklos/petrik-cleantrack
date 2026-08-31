using CleanTrack.Api.Data;
using CleanTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CleanTrack.Api.Services;

public class AttendanceService(AppDbContext db, TimeService time)
{
    public Task<AttendanceEvent?> GetLastEventAsync(Guid employeeId) => db.AttendanceEvents
        .Where(x => x.EmployeeId == employeeId)
        .OrderByDescending(x => x.OccurredAtUtc)
        .FirstOrDefaultAsync();

    public async Task<AttendanceEventType> GetNextActionAsync(Guid employeeId)
    {
        var last = await GetLastEventAsync(employeeId);
        return last?.Type == AttendanceEventType.CheckIn
            ? AttendanceEventType.CheckOut
            : AttendanceEventType.CheckIn;
    }

    public async Task<int> GetWorkedMinutesForDayAsync(Guid employeeId, DateTime utcReference)
    {
        var (from, to) = time.GetLocalDayUtcRange(utcReference);
        var events = await db.AttendanceEvents
            .Where(x => x.EmployeeId == employeeId && x.OccurredAtUtc >= from && x.OccurredAtUtc < to)
            .OrderBy(x => x.OccurredAtUtc)
            .ToListAsync();

        var total = TimeSpan.Zero;
        DateTime? open = null;
        foreach (var e in events)
        {
            if (e.Type == AttendanceEventType.CheckIn)
                open = e.OccurredAtUtc;
            else if (e.Type == AttendanceEventType.CheckOut && open.HasValue && e.OccurredAtUtc >= open.Value)
            {
                total += e.OccurredAtUtc - open.Value;
                open = null;
            }
        }
        return (int)Math.Floor(total.TotalMinutes);
    }
}
