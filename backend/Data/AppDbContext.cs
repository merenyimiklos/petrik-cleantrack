using CleanTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CleanTrack.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Terminal> Terminals => Set<Terminal>();
    public DbSet<AttendanceEvent> AttendanceEvents => Set<AttendanceEvent>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Employee>().HasIndex(x => x.EmployeeCode).IsUnique();
        modelBuilder.Entity<Employee>().HasIndex(x => x.RfidUid).IsUnique();
        modelBuilder.Entity<Terminal>().HasIndex(x => x.DeviceId).IsUnique();
        modelBuilder.Entity<AttendanceEvent>().HasIndex(x => x.ExternalEventId).IsUnique();
        modelBuilder.Entity<AttendanceEvent>().HasIndex(x => new { x.EmployeeId, x.OccurredAtUtc });

        modelBuilder.Entity<AttendanceEvent>()
            .HasOne(x => x.Employee)
            .WithMany(x => x.AttendanceEvents)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AttendanceEvent>()
            .HasOne(x => x.Terminal)
            .WithMany(x => x.AttendanceEvents)
            .HasForeignKey(x => x.TerminalId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
