namespace Assignment_Example_HU.Application.DTOs.Response;

public class GameResponse
{
    public int GameId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int VenueId { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public int CourtId { get; set; }
    public string CourtName { get; set; } = string.Empty;
    public int CreatedBy { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public int CurrentPlayers { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ParticipantResponse> Participants { get; set; } = new();
}

public class ParticipantResponse
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public decimal Rating { get; set; }
}
