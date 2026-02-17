using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Infrastructure.Repositories;

namespace Assignment_Example_HU.Application.Services;

public class SlotService : ISlotService
{
    private readonly IRepository<Court> _courtRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IPricingService _pricingService;

    public SlotService(
        IRepository<Court> courtRepository,
        IBookingRepository bookingRepository,
        IPricingService pricingService)
    {
        _courtRepository = courtRepository;
        _bookingRepository = bookingRepository;
        _pricingService = pricingService;
    }

    public async Task<IEnumerable<SlotAvailabilityResponse>> GetAvailableSlotsAsync(int courtId, DateTime date)
    {
        var court = await _courtRepository.GetByIdAsync(courtId);
        if (court == null)
            throw new NotFoundException("Court", courtId);

        // Parse operating hours
        var openTime = TimeSpan.Parse(court.OpenTime);
        var closeTime = TimeSpan.Parse(court.CloseTime);

        // Generate all possible slots for the day
        var slots = new List<SlotAvailabilityResponse>();
        var currentSlotStart = date.Date.Add(openTime);
        var endOfDay = date.Date.Add(closeTime);

        while (currentSlotStart.Add(TimeSpan.FromMinutes(court.SlotDurationMinutes)) <= endOfDay)
        {
            var currentSlotEnd = currentSlotStart.Add(TimeSpan.FromMinutes(court.SlotDurationMinutes));

            // Check if slot is in the past
            if (currentSlotStart <= DateTime.UtcNow)
            {
                currentSlotStart = currentSlotEnd;
                continue;
            }

            // Check availability
            var isAvailable = await _bookingRepository.IsSlotAvailableAsync(
                courtId,
                currentSlotStart,
                currentSlotEnd
            );

            // Track view and calculate price
            await _pricingService.TrackSlotViewAsync(courtId, currentSlotStart);
            var pricingBreakdown = await _pricingService.GetPricingBreakdownAsync(
                courtId,
                currentSlotStart,
                currentSlotEnd
            );

            slots.Add(new SlotAvailabilityResponse
            {
                CourtId = courtId,
                CourtName = court.Name,
                SlotStartTime = currentSlotStart,
                SlotEndTime = currentSlotEnd,
                BasePrice = court.BasePrice,
                FinalPrice = pricingBreakdown.FinalPrice,
                IsAvailable = isAvailable,
                PricingBreakdown = pricingBreakdown
            });

            currentSlotStart = currentSlotEnd;
        }

        return slots;
    }

    public async Task<SlotAvailabilityResponse?> GetSlotDetailsAsync(int courtId, DateTime slotStartTime, DateTime slotEndTime)
    {
        var court = await _courtRepository.GetByIdAsync(courtId);
        if (court == null)
            throw new NotFoundException("Court", courtId);

        // Check if slot is in the past
        if (slotStartTime <= DateTime.UtcNow)
            throw new BusinessException("Cannot get details for a slot in the past");

        // Check if slot is within operating hours
        var openTime = TimeSpan.Parse(court.OpenTime);
        var closeTime = TimeSpan.Parse(court.CloseTime);
        var slotStartTimeOfDay = slotStartTime.TimeOfDay;
        var slotEndTimeOfDay = slotEndTime.TimeOfDay;

        if (slotStartTimeOfDay < openTime || slotEndTimeOfDay > closeTime)
            throw new BusinessException("Slot is outside court operating hours");

        // Check availability
        var isAvailable = await _bookingRepository.IsSlotAvailableAsync(
            courtId,
            slotStartTime,
            slotEndTime
        );

        // Track view and calculate price
        await _pricingService.TrackSlotViewAsync(courtId, slotStartTime);
        var pricingBreakdown = await _pricingService.GetPricingBreakdownAsync(
            courtId,
            slotStartTime,
            slotEndTime
        );

        return new SlotAvailabilityResponse
        {
            CourtId = courtId,
            CourtName = court.Name,
            SlotStartTime = slotStartTime,
            SlotEndTime = slotEndTime,
            BasePrice = court.BasePrice,
            FinalPrice = pricingBreakdown.FinalPrice,
            IsAvailable = isAvailable,
            PricingBreakdown = pricingBreakdown
        };
    }
}
