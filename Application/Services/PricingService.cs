using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Constants;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Infrastructure.Caching;
using Assignment_Example_HU.Infrastructure.Repositories;

namespace Assignment_Example_HU.Application.Services;

public class PricingService : IPricingService
{
    private readonly IRepository<Court> _courtRepository;
    private readonly IDiscountService _discountService;
    private readonly IBookingRepository _bookingRepository;
    private readonly IRepository<Rating> _ratingRepository;
    private readonly ICacheService _cacheService;

    public PricingService(
        IRepository<Court> courtRepository,
        IDiscountService discountService,
        IBookingRepository bookingRepository,
        IRepository<Rating> ratingRepository,
        ICacheService cacheService)
    {
        _courtRepository = courtRepository;
        _discountService = discountService;
        _bookingRepository = bookingRepository;
        _ratingRepository = ratingRepository;
        _cacheService = cacheService;
    }

    public async Task<decimal> CalculateDynamicPriceAsync(int courtId, DateTime slotStartTime, DateTime slotEndTime)
    {
        var breakdown = await GetPricingBreakdownAsync(courtId, slotStartTime, slotEndTime);
        return breakdown.FinalPrice;
    }

    public async Task<PricingBreakdown> GetPricingBreakdownAsync(int courtId, DateTime slotStartTime, DateTime slotEndTime)
    {
        var court = await _courtRepository.GetByIdAsync(courtId);
        if (court == null)
            throw new NotFoundException("Court", courtId);

        var basePrice = court.BasePrice;

        // Calculate demand multiplier
        var demandMultiplier = await CalculateDemandMultiplierAsync(courtId, slotStartTime);

        // Calculate time-based multiplier
        var timeMultiplier = CalculateTimeBasedMultiplier(slotStartTime);

        // Calculate historical multiplier
        var historicalMultiplier = await CalculateHistoricalMultiplierAsync(courtId);

        // Calculate discount factor
        var discountFactor = 1.0m;
        var discountAmount = 0m;
        var discount = await _discountService.GetApplicableDiscountAsync(court.VenueId, courtId, slotStartTime);
        if (discount != null)
        {
            discountFactor = 1 - (discount.PercentOff / 100);
        }

        // Calculate final price
        var priceBeforeDiscount = basePrice * demandMultiplier * timeMultiplier * historicalMultiplier;
        var finalPrice = priceBeforeDiscount * discountFactor;
        discountAmount = priceBeforeDiscount - finalPrice;

        return new PricingBreakdown
        {
            BasePrice = basePrice,
            DemandMultiplier = demandMultiplier,
            TimeMultiplier = timeMultiplier,
            HistoricalMultiplier = historicalMultiplier,
            DiscountFactor = discountFactor,
            DiscountAmount = discountAmount,
            FinalPrice = Math.Round(finalPrice, 2)
        };
    }

    public async Task TrackSlotViewAsync(int courtId, DateTime slotStartTime)
    {
        var key = $"slot_views:{courtId}:{slotStartTime:yyyyMMddHHmm}";
        await _cacheService.IncrementAsync(key, 1);
        await _cacheService.SetAsync(key, await _cacheService.GetAsync<long>(key), TimeSpan.FromMinutes(10));
    }

    public async Task<long> GetSlotViewCountAsync(int courtId, DateTime slotStartTime)
    {
        var key = $"slot_views:{courtId}:{slotStartTime:yyyyMMddHHmm}";
        var count = await _cacheService.GetAsync<long>(key);
        return count;
    }

    private async Task<decimal> CalculateDemandMultiplierAsync(int courtId, DateTime slotStartTime)
    {
        var viewCount = await GetSlotViewCountAsync(courtId, slotStartTime);

        if (viewCount == 0)
            return PricingConstants.DemandMultiplier_NoViewers;
        if (viewCount >= 2 && viewCount <= 5)
            return PricingConstants.DemandMultiplier_2To5Viewers;
        if (viewCount > 5)
            return PricingConstants.DemandMultiplier_MoreThan5Viewers;

        return PricingConstants.DemandMultiplier_NoViewers;
    }

    private decimal CalculateTimeBasedMultiplier(DateTime slotStartTime)
    {
        var hoursUntilSlot = (slotStartTime - DateTime.UtcNow).TotalHours;

        if (hoursUntilSlot > 24)
            return PricingConstants.TimeMultiplier_MoreThan24Hours;
        if (hoursUntilSlot >= 6 && hoursUntilSlot <= 24)
            return PricingConstants.TimeMultiplier_6To24Hours;
        if (hoursUntilSlot < 6 && hoursUntilSlot > 0)
            return PricingConstants.TimeMultiplier_LessThan6Hours;

        return PricingConstants.TimeMultiplier_MoreThan24Hours;
    }

    private async Task<decimal> CalculateHistoricalMultiplierAsync(int courtId)
    {
        var ratings = await _ratingRepository.FindAsync(r => r.CourtId == courtId);

        if (!ratings.Any())
            return PricingConstants.HistoricalMultiplier_Low;

        var averageRating = ratings.Average(r => r.Score);

        if (averageRating >= 4)
            return PricingConstants.HistoricalMultiplier_High;
        if (averageRating == 3)
            return PricingConstants.HistoricalMultiplier_Medium;

        return PricingConstants.HistoricalMultiplier_Low;
    }
}
