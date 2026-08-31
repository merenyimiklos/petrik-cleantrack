using System.Security.Claims;
using System.Text.Json;
using CleanTrack.Api.Data;
using CleanTrack.Api.Models;

namespace CleanTrack.Api.Services;

public class AuditService(AppDbContext db)
{
    public async Task WriteAsync(ClaimsPrincipal? user, string action, string entityType, string entityId, object? details = null)
    {
        Guid? userId = null;
        var id = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(id, out var parsed)) userId = parsed;

        db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            DetailsJson = details is null ? null : JsonSerializer.Serialize(details)
        });
        await db.SaveChangesAsync();
    }
}
