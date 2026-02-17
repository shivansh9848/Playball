using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;

namespace Assignment_Example_HU.Application.Interfaces;

public interface IDiscountService
{
    Task<DiscountResponse> CreateDiscountAsync(int ownerId, CreateDiscountRequest request);
    Task<IEnumerable<DiscountResponse>> GetMyDiscountsAsync(int ownerId);
    Task<IEnumerable<DiscountResponse>> GetActiveDiscountsAsync();
    Task<DiscountResponse?> GetApplicableDiscountAsync(int? venueId, int? courtId, DateTime slotTime);
    Task<IEnumerable<DiscountResponse>> GetDiscountsByVenueAsync(int venueId);
    Task<IEnumerable<DiscountResponse>> GetDiscountsByCourtAsync(int courtId);
}
