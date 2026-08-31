using CleanTrack.Api.Data;
using CleanTrack.Api.Dtos;
using CleanTrack.Api.Models;
using CleanTrack.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanTrack.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/attendance")]
public class AttendanceController(AppDbContext db, AuditService audit) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] Guid? employeeId)
    {
        var query = db.AttendanceEvents.AsNoTracking();
        if (from.HasValue) query = query.Where(x => x.OccurredAtUtc >= from.Value.ToUniversalTime());
        if (to.HasValue) query = query.Where(x => x.OccurredAtUtc < to.Value.ToUniversalTime());
        if (employeeId.HasValue) query = query.Where(x => x.EmployeeId == employeeId.Value);

        var rows = await query
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(1000)
            .Select(x => new
            {
                x.Id,
                x.EmployeeId,
                EmployeeName = x.Employee.FullName,
                x.Type,
                x.Source,
                x.OccurredAtUtc,
                TerminalName = x.Terminal != null ? x.Terminal.Name : null,
                x.Note
            })
            .ToListAsync();
        return Ok(rows);
    }

    [HttpPost("manual")]
    public async Task<IActionResult> AddManual(ManualAttendanceRequest request)
    {
        var employee = await db.Employees.FindAsync(request.EmployeeId);
        if (employee is null) return NotFound(new { message = "A dolgozó nem található." });

        var evt = new AttendanceEvent
        {
            EmployeeId = request.EmployeeId,
            Type = request.Type,
            Source = AttendanceSource.Admin,
            OccurredAtUtc = request.OccurredAt.UtcDateTime,
            Note = request.Note
        };
        db.AttendanceEvents.Add(evt);
        await db.SaveChangesAsync();
        await audit.WriteAsync(User, "attendance.manual.create", nameof(AttendanceEvent), evt.Id.ToString(), new { evt.EmployeeId, evt.Type, evt.OccurredAtUtc, evt.Note });
        return Ok(evt);
    }
}
