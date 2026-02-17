using Assignment_Example_HU.Application.DTOs.Response;

namespace Assignment_Example_HU.Application.Interfaces;

public interface IPricingService
{
    Task<decimal> CalculateDynamicPriceAsync(int courtId, DateTime slotStartTime, DateTime slotEndTime);
    Task<PricingBreakdown> GetPricingBreakdownAsync(int courtId, DateTime slotStartTime, DateTime slotEndTime);
    Task TrackSlotViewAsync(int courtId, DateTime slotStartTime);
    Task<long> GetSlotViewCountAsync(int courtId, DateTime slotStartTime);
}
