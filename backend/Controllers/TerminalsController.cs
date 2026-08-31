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
[Route("api/terminals")]
public class TerminalsController(AppDbContext db, DeviceKeyService keys, AuditService audit) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await db.Terminals
        .OrderBy(x => x.Name)
        .Select(x => new { x.Id, x.DeviceId, x.Name, x.IsActive, x.LastSeenAtUtc, x.CreatedAtUtc })
        .ToListAsync());

    [HttpPost]
    public async Task<ActionResult<CreateTerminalResponse>> Create(CreateTerminalRequest request)
    {
        var deviceId = request.DeviceId.Trim().ToUpperInvariant();
        if (await db.Terminals.AnyAsync(x => x.DeviceId == deviceId))
            return Conflict(new { message = "Ilyen Device ID már létezik." });

        var apiKey = keys.GenerateKey();
        var terminal = new Terminal
        {
            DeviceId = deviceId,
            Name = request.Name.Trim(),
            ApiKeyHash = keys.Hash(apiKey)
        };
        db.Terminals.Add(terminal);
        await db.SaveChangesAsync();
        await audit.WriteAsync(User, "terminal.create", nameof(Terminal), terminal.Id.ToString(), new { terminal.DeviceId, terminal.Name });
        return Ok(new CreateTerminalResponse(terminal.Id, terminal.DeviceId, terminal.Name, apiKey));
    }

    [HttpPost("{id:guid}/regenerate-key")]
    public async Task<IActionResult> RegenerateKey(Guid id)
    {
        var terminal = await db.Terminals.FindAsync(id);
        if (terminal is null) return NotFound();
        var apiKey = keys.GenerateKey();
        terminal.ApiKeyHash = keys.Hash(apiKey);
        await db.SaveChangesAsync();
        await audit.WriteAsync(User, "terminal.key.regenerate", nameof(Terminal), terminal.Id.ToString());
        return Ok(new { apiKey });
    }
}
