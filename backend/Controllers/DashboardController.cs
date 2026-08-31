using CleanTrack.Api.Data;
using CleanTrack.Api.Models;
using CleanTrack.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanTrack.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/dashboard")]
public class DashboardController(AppDbContext db, TimeService time) : ControllerBase
{
    [HttpGet("today")]
    public async Task<IActionResult> Today()
    {
        var (from, to) = time.GetLocalDayUtcRange(DateTime.UtcNow);
        var activeEmployees = await db.Employees.CountAsync(x => x.IsActive);
        var todayEvents = await db.AttendanceEvents
            .Where(x => x.OccurredAtUtc >= from && x.OccurredAtUtc < to)
            .OrderBy(x => x.OccurredAtUtc)
            .Select(x => new { x.EmployeeId, EmployeeName = x.Employee.FullName, x.Type, x.OccurredAtUtc })
            .ToListAsync();

        var rows = todayEvents
            .GroupBy(x => new { x.EmployeeId, x.EmployeeName })
            .Select(g =>
            {
                var ordered = g.OrderBy(x => x.OccurredAtUtc).ToList();
                var firstIn = ordered.FirstOrDefault(x => x.Type == AttendanceEventType.CheckIn)?.OccurredAtUtc;
                var last = ordered.Last();
                return new
                {
                    g.Key.EmployeeId,
                    g.Key.EmployeeName,
                    FirstCheckInUtc = firstIn,
                    LastEventUtc = last.OccurredAtUtc,
                    LastEventType = last.Type,
                    IsPresent = last.Type == AttendanceEventType.CheckIn
                };
            })
            .OrderBy(x => x.EmployeeName)
            .ToList();

        return Ok(new
        {
            dayStartUtc = from,
            activeEmployees,
            present = rows.Count(x => x.IsPresent),
            checkedToday = rows.Count,
            eventsToday = todayEvents.Count,
            rows
        });
    }
}
