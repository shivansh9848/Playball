using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;

namespace Assignment_Example_HU.Application.Interfaces;

public interface IRatingService
{
    Task<RatingResponse> RateVenueAsync(int userId, int venueId, CreateRatingRequest request);
    Task<RatingResponse> RateCourtAsync(int userId, int courtId, CreateRatingRequest request);
    Task<RatingResponse> RatePlayerAsync(int userId, int targetUserId, CreateRatingRequest request);
    Task<IEnumerable<RatingResponse>> GetVenueRatingsAsync(int venueId);
    Task<IEnumerable<RatingResponse>> GetCourtRatingsAsync(int courtId);
    Task<IEnumerable<RatingResponse>> GetPlayerRatingsAsync(int playerId);
    Task<decimal> GetAverageRatingAsync(int? venueId = null, int? courtId = null, int? playerId = null);
}
