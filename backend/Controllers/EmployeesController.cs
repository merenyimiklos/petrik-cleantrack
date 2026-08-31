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
[Route("api/employees")]
public class EmployeesController(AppDbContext db, AuditService audit) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await db.Employees.OrderBy(x => x.FullName).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(EmployeeRequest request)
    {
        var employee = new Employee
        {
            EmployeeCode = request.EmployeeCode.Trim(),
            FullName = request.FullName.Trim(),
            DailyTargetMinutes = request.DailyTargetMinutes,
            IsActive = request.IsActive
        };
        db.Employees.Add(employee);
        await db.SaveChangesAsync();
        await audit.WriteAsync(User, "employee.create", nameof(Employee), employee.Id.ToString(), new { employee.EmployeeCode, employee.FullName });
        return CreatedAtAction(nameof(List), new { id = employee.Id }, employee);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, EmployeeRequest request)
    {
        var employee = await db.Employees.FindAsync(id);
        if (employee is null) return NotFound();

        employee.EmployeeCode = request.EmployeeCode.Trim();
        employee.FullName = request.FullName.Trim();
        employee.DailyTargetMinutes = request.DailyTargetMinutes;
        employee.IsActive = request.IsActive;
        employee.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await audit.WriteAsync(User, "employee.update", nameof(Employee), employee.Id.ToString());
        return Ok(employee);
    }

    [HttpPut("{id:guid}/rfid")]
    public async Task<IActionResult> AssignRfid(Guid id, AssignRfidRequest request)
    {
        var employee = await db.Employees.FindAsync(id);
        if (employee is null) return NotFound();
        var uid = NormalizeUid(request.RfidUid);
        if (string.IsNullOrWhiteSpace(uid)) return BadRequest(new { message = "Érvénytelen RFID UID." });

        var conflict = await db.Employees.AnyAsync(x => x.Id != id && x.RfidUid == uid);
        if (conflict) return Conflict(new { message = "Ez az RFID kártya már egy másik dolgozóhoz tartozik." });

        employee.RfidUid = uid;
        employee.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await audit.WriteAsync(User, "employee.rfid.assign", nameof(Employee), employee.Id.ToString(), new { RfidUid = uid });
        return Ok(employee);
    }

    private static string NormalizeUid(string uid) =>
        new(uid.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
}
