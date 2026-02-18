using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;
using Assignment_Example_HU.Common.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Assignment_Example_HU.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAdminAsync(ApplicationDbContext context)
    {
        // Only seed if no admin exists
        var adminExists = await context.Users.AnyAsync(u => u.Role == UserRole.Admin);
        if (adminExists) return;

        var admin = new User
        {
            FullName = "Platform Admin",
            Email = "admin@playball.com",
            PhoneNumber = "9000000000",
            PasswordHash = PasswordHasher.HashPassword("Admin@123"),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(admin);
        await context.SaveChangesAsync();

        // Create wallet for admin
        var wallet = new Wallet
        {
            UserId = admin.UserId,
            Balance = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Set<Wallet>().Add(wallet);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Default admin seeded: admin@playball.com / Admin@123");
    }
}
