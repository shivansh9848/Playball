using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;

namespace Assignment_Example_HU.Application.Interfaces;

public interface ICourtService
{
    Task<CourtResponse> CreateCourtAsync(int ownerId, CreateCourtRequest request);
    Task<CourtResponse> UpdateCourtAsync(int ownerId, int courtId, UpdateCourtRequest request);
    Task DeleteCourtAsync(int ownerId, int courtId);
    Task<IEnumerable<CourtResponse>> GetCourtsByVenueAsync(int venueId);
    Task<CourtResponse?> GetCourtByIdAsync(int courtId);
}
