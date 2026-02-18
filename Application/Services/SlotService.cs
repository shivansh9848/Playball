using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Common.Helpers;
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

    /// <summary>
    /// Ensures a DateTime has Kind=Utc (required by PostgreSQL 'timestamp with time zone').
    /// </summary>
    private static DateTime EnsureUtc(DateTime dt)
    {
        return dt.Kind == DateTimeKind.Utc
            ? dt
            : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    }

    public async Task<IEnumerable<SlotAvailabilityResponse>> GetAvailableSlotsAsync(int courtId, DateTime date)
    {
        var court = await _courtRepository.GetByIdAsync(courtId);
        if (court == null)
            throw new NotFoundException("Court", courtId);

        // Parse operating hours (stored as local time strings, e.g. "06:00", "22:00")
        var openTime = TimeSpan.Parse(court.OpenTime);
        var closeTime = TimeSpan.Parse(court.CloseTime);

        // Generate slots — ensure all DateTimes are UTC for PostgreSQL compatibility
        var slots = new List<SlotAvailabilityResponse>();
        var currentSlotStart = EnsureUtc(date.Date.Add(openTime));
        var endOfDay = EnsureUtc(date.Date.Add(closeTime));
        var now = DateTime.UtcNow;

        while (currentSlotStart.Add(TimeSpan.FromMinutes(court.SlotDurationMinutes)) <= endOfDay)
        {
            var currentSlotEnd = currentSlotStart.Add(TimeSpan.FromMinutes(court.SlotDurationMinutes));

            // Skip slots that are in the past
            if (currentSlotStart <= now)
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

        // Ensure UTC for PostgreSQL compatibility
        slotStartTime = EnsureUtc(slotStartTime);
        slotEndTime = EnsureUtc(slotEndTime);

        var now = DateTime.UtcNow;

        // Check if slot is in the past
        if (slotStartTime <= now)
            throw new BusinessException("Cannot get details for a slot in the past");

        // Validate slot duration
        if (slotEndTime <= slotStartTime)
            throw new BusinessException("Slot end time must be after start time");

        // Check if slot is within operating hours
        var openTime = TimeSpan.Parse(court.OpenTime);
        var closeTime = TimeSpan.Parse(court.CloseTime);
        var slotStartTimeOfDay = slotStartTime.TimeOfDay;
        var slotEndTimeOfDay = slotEndTime.TimeOfDay;

        if (slotStartTimeOfDay < openTime || slotEndTimeOfDay > closeTime)
            throw new BusinessException(
                $"Slot is outside court operating hours ({court.OpenTime} - {court.CloseTime}). " +
                $"Your request: {slotStartTime:HH:mm} - {slotEndTime:HH:mm}");

        // Validate slot duration matches court configuration
        var expectedDuration = TimeSpan.FromMinutes(court.SlotDurationMinutes);
        var actualDuration = slotEndTime - slotStartTime;
        if (actualDuration != expectedDuration)
            throw new BusinessException(
                $"Slot duration must be {court.SlotDurationMinutes} minutes. " +
                $"Your request spans {actualDuration.TotalMinutes} minutes.");

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
