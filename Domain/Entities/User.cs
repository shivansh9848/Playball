using System.ComponentModel.DataAnnotations;
using Assignment_Example_HU.Domain.Enums;

namespace Assignment_Example_HU.Domain.Entities;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(15)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } = UserRole.User;



    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    // Player profile fields
    public decimal AggregatedRating { get; set; } = 0;
    public int GamesPlayed { get; set; } = 0;
    public string PreferredSports { get; set; } = string.Empty; // Comma-separated SportType values

    // Navigation properties
    public virtual Wallet? Wallet { get; set; }
    public virtual ICollection<Venue> OwnedVenues { get; set; } = new List<Venue>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<Game> CreatedGames { get; set; } = new List<Game>();
    public virtual ICollection<Rating> GivenRatings { get; set; } = new List<Rating>();
}
