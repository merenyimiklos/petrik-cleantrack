using CleanTrack.Api.Data;
using CleanTrack.Api.Dtos;
using CleanTrack.Api.Models;
using CleanTrack.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CleanTrack.Api.Controllers;

[ApiController]
[Route("api/terminal")]
public class TerminalApiController(AppDbContext db, DeviceKeyService keys, AttendanceService attendance) : ControllerBase
{
    [HttpPost("scan")]
    public async Task<ActionResult<TerminalScanResponse>> Scan(TerminalScanRequest request)
    {
        var terminal = await AuthenticateTerminalAsync();
        if (terminal is null) return Unauthorized(new { message = "Érvénytelen terminál hitelesítés." });

        var existing = await db.AttendanceEvents
            .Include(x => x.Employee)
            .SingleOrDefaultAsync(x => x.ExternalEventId == request.EventId);
        if (existing is not null)
        {
            var minutes = await attendance.GetWorkedMinutesForDayAsync(existing.EmployeeId, existing.OccurredAtUtc);
            return Ok(new TerminalScanResponse(true, existing.Employee.FullName, existing.Type, existing.OccurredAtUtc, minutes, true));
        }

        var uid = NormalizeUid(request.RfidUid);
        var employee = await db.Employees.SingleOrDefaultAsync(x => x.RfidUid == uid && x.IsActive);
        if (employee is null) return NotFound(new { message = "Ismeretlen vagy inaktív RFID kártya." });

        var occurred = request.OccurredAt?.UtcDateTime ?? DateTime.UtcNow;
        if (occurred > DateTime.UtcNow.AddMinutes(5) || occurred < DateTime.UtcNow.AddDays(-14))
            return BadRequest(new { message = "Az esemény időpontja kívül esik az elfogadott tartományon." });

        var lastEvent = await attendance.GetLastEventAsync(employee.Id);
        if (lastEvent is not null && Math.Abs((occurred - lastEvent.OccurredAtUtc).TotalSeconds) < 10)
        {
            var lastMinutes = await attendance.GetWorkedMinutesForDayAsync(employee.Id, lastEvent.OccurredAtUtc);
            return Ok(new TerminalScanResponse(true, employee.FullName, lastEvent.Type, lastEvent.OccurredAtUtc, lastMinutes, true));
        }

        var action = await attendance.GetNextActionAsync(employee.Id);
        var evt = new AttendanceEvent
        {
            EmployeeId = employee.Id,
            TerminalId = terminal.Id,
            Type = action,
            Source = AttendanceSource.Terminal,
            ExternalEventId = request.EventId,
            OccurredAtUtc = occurred
        };
        db.AttendanceEvents.Add(evt);
        terminal.LastSeenAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var workedMinutes = await attendance.GetWorkedMinutesForDayAsync(employee.Id, occurred);
        return Ok(new TerminalScanResponse(true, employee.FullName, action, occurred, workedMinutes, false));
    }

    [HttpGet("ping")]
    public async Task<IActionResult> Ping()
    {
        var terminal = await AuthenticateTerminalAsync();
        if (terminal is null) return Unauthorized();
        terminal.LastSeenAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(new { serverTimeUtc = DateTime.UtcNow, terminal = terminal.Name });
    }

    private async Task<Terminal?> AuthenticateTerminalAsync()
    {
        if (!Request.Headers.TryGetValue("X-Device-Id", out var deviceId) ||
            !Request.Headers.TryGetValue("X-Device-Key", out var apiKey)) return null;

        var normalized = deviceId.ToString().Trim().ToUpperInvariant();
        var terminal = await db.Terminals.SingleOrDefaultAsync(x => x.DeviceId == normalized && x.IsActive);
        if (terminal is null || !keys.Verify(apiKey.ToString(), terminal.ApiKeyHash)) return null;
        return terminal;
    }

    private static string NormalizeUid(string uid) =>
        new(uid.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
}
