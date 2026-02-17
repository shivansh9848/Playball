namespace Assignment_Example_HU.Application.DTOs.Response;

public class WaitlistResponse
{
    public int WaitlistId { get; set; }
    public int GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal UserRating { get; set; }
    public int Position { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsInvited { get; set; }
}
