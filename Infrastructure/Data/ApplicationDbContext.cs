using Microsoft.EntityFrameworkCore;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Domain.Enums;

namespace Assignment_Example_HU.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<Court> Courts { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<GameParticipant> GameParticipants { get; set; }
    public DbSet<Waitlist> Waitlists { get; set; }
    public DbSet<Rating> Ratings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.PhoneNumber);

            entity.HasOne(e => e.Wallet)
                .WithOne(w => w.User)
                .HasForeignKey<Wallet>(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.OwnedVenues)
                .WithOne(v => v.Owner)
                .HasForeignKey(v => v.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Bookings)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.CreatedGames)
                .WithOne(g => g.Creator)
                .HasForeignKey(g => g.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Wallet configuration
        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.WalletId);
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // Transaction configuration
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId);
            entity.HasIndex(e => e.ReferenceId);
            entity.HasIndex(e => new { e.WalletId, e.CreatedAt });
        });

        // Venue configuration
        modelBuilder.Entity<Venue>(entity =>
        {
            entity.HasKey(e => e.VenueId);
            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.ApprovalStatus);

            entity.HasMany(e => e.Courts)
                .WithOne(c => c.Venue)
                .HasForeignKey(c => c.VenueId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Court configuration
        modelBuilder.Entity<Court>(entity =>
        {
            entity.HasKey(e => e.CourtId);
            entity.HasIndex(e => e.VenueId);
            entity.HasIndex(e => new { e.VenueId, e.IsActive });
        });

        // Discount configuration
        modelBuilder.Entity<Discount>(entity =>
        {
            entity.HasKey(e => e.DiscountId);
            entity.HasIndex(e => new { e.VenueId, e.IsActive });
            entity.HasIndex(e => new { e.CourtId, e.IsActive });
            entity.HasIndex(e => new { e.ValidFrom, e.ValidTo });
        });

        // Booking configuration
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.CourtId, e.SlotStartTime, e.SlotEndTime });
            entity.HasIndex(e => new { e.Status, e.SlotStartTime });
            entity.HasIndex(e => e.LockExpiryTime);
        });

        // Game configuration
        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.GameId);
            entity.HasIndex(e => new { e.Status, e.StartTime });
            entity.HasIndex(e => e.CreatedBy);
        });

        // GameParticipant configuration
        modelBuilder.Entity<GameParticipant>(entity =>
        {
            entity.HasKey(e => e.GameParticipantId);
            entity.HasIndex(e => new { e.GameId, e.UserId }).IsUnique();
        });

        // Waitlist configuration
        modelBuilder.Entity<Waitlist>(entity =>
        {
            entity.HasKey(e => e.WaitlistId);
            entity.HasIndex(e => new { e.GameId, e.UserId }).IsUnique();
            entity.HasIndex(e => new { e.GameId, e.Position });
        });

        // Rating configuration
        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.RatingId);
            entity.HasIndex(e => new { e.GameId, e.UserId, e.TargetType, e.VenueId, e.CourtId, e.TargetUserId }).IsUnique();
            entity.HasIndex(e => e.VenueId);
            entity.HasIndex(e => e.CourtId);
            entity.HasIndex(e => e.TargetUserId);

            entity.HasOne(e => e.TargetUser)
                .WithMany()
                .HasForeignKey(e => e.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed initial admin user
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed an admin user with default password: Admin@123
        // Pre-generated BCrypt hash for "Admin@123"
        var adminUser = new User
        {
            UserId = 1,
            FullName = "System Admin",
            Email = "admin@playball.com",
            PhoneNumber = "1234567890",
            // Password: Admin@123 (pre-hashed with BCrypt)
            PasswordHash = "$2a$11$xK9j5L6rE8QY7X3ZB4N9Ou.XYHZqXqFJKvYzN5LQpW7WMqGKE9/va",
            Role = UserRole.Admin,

            CreatedAt = new DateTime(2026, 2, 17, 0, 0, 0, DateTimeKind.Utc)
        };

        modelBuilder.Entity<User>().HasData(adminUser);

        // Seed wallet for admin
        var adminWallet = new Wallet
        {
            WalletId = 1,
            UserId = 1,
            Balance = 0,
            CreatedAt = new DateTime(2026, 2, 17, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 2, 17, 0, 0, 0, DateTimeKind.Utc)
        };

        modelBuilder.Entity<Wallet>().HasData(adminWallet);
    }
}
