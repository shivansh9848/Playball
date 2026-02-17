namespace Assignment_Example_HU.Application.DTOs.Response;

public class PlayerProfileResponse
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int TotalGamesPlayed { get; set; }
    public int TotalRatingsReceived { get; set; }
    public List<RatingResponse> RecentReviews { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
