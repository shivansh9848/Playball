namespace Assignment_Example_HU.Application.DTOs.Response;

public class UserProfileResponse
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public decimal AggregatedRating { get; set; }
    public int GamesPlayed { get; set; }
    public string PreferredSports { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
