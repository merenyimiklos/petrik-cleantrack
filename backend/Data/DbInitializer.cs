using CleanTrack.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanTrack.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // MVP bootstrap: create schema automatically. Replace with EF migrations before production rollout.
        await db.Database.EnsureCreatedAsync();

        if (await db.Users.AnyAsync()) return;

        var email = configuration["BootstrapAdmin:Email"]
            ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL")
            ?? "admin@petrik.hu";
        var password = configuration["BootstrapAdmin:Password"]
            ?? Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
            ?? "ChangeMe123!";
        var fullName = configuration["BootstrapAdmin:FullName"]
            ?? Environment.GetEnvironmentVariable("ADMIN_FULL_NAME")
            ?? "CleanTrack Admin";

        var user = new AppUser
        {
            Email = email.Trim().ToLowerInvariant(),
            FullName = fullName,
            Role = "Admin"
        };

        user.PasswordHash = new PasswordHasher<AppUser>().HashPassword(user, password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
}
