namespace Assignment_Example_HU.Application.DTOs.Response;

public class RatingResponse
{
    public int RatingId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int GameId { get; set; }
    public string TargetType { get; set; } = string.Empty; // "Venue", "Court", "Player"
    public int? VenueId { get; set; }
    public string? VenueName { get; set; }
    public int? CourtId { get; set; }
    public string? CourtName { get; set; }
    public int? TargetUserId { get; set; }
    public string? TargetUserName { get; set; }
    public int Score { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
