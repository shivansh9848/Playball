using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;

namespace Assignment_Example_HU.Application.Interfaces;

public interface IVenueService
{
    Task<VenueResponse> CreateVenueAsync(int ownerId, CreateVenueRequest request);
    Task<VenueResponse> ApproveVenueAsync(int adminId, int venueId, ApproveVenueRequest request);
    Task<IEnumerable<VenueResponse>> GetAllVenuesAsync();
    Task<IEnumerable<VenueResponse>> GetMyVenuesAsync(int ownerId);
    Task<IEnumerable<VenueResponse>> GetPendingVenuesAsync();
    Task<VenueResponse?> GetVenueByIdAsync(int venueId);
}
