using Assignment_Example_HU.Application.DTOs.Response;

namespace Assignment_Example_HU.Application.Interfaces;

public interface ISlotService
{
    Task<IEnumerable<SlotAvailabilityResponse>> GetAvailableSlotsAsync(int courtId, DateTime date);
    Task<SlotAvailabilityResponse?> GetSlotDetailsAsync(int courtId, DateTime slotStartTime, DateTime slotEndTime);
}
