using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Infrastructure.Repositories;

namespace Assignment_Example_HU.Application.Services;

public class DiscountService : IDiscountService
{
    private readonly IRepository<Discount> _discountRepository;
    private readonly IVenueRepository _venueRepository;
    private readonly IRepository<Court> _courtRepository;

    public DiscountService(
        IRepository<Discount> discountRepository,
        IVenueRepository venueRepository,
        IRepository<Court> courtRepository)
    {
        _discountRepository = discountRepository;
        _venueRepository = venueRepository;
        _courtRepository = courtRepository;
    }

    public async Task<DiscountResponse> CreateDiscountAsync(int ownerId, CreateDiscountRequest request)
    {
        if (request.ValidFrom >= request.ValidTo)
            throw new BusinessException("ValidFrom must be before ValidTo");

        if (request.Scope == "Venue")
        {
            if (!request.VenueId.HasValue)
                throw new BusinessException("VenueId is required for venue-scoped discounts");

            var venue = await _venueRepository.GetByIdAsync(request.VenueId.Value);
            if (venue == null)
                throw new NotFoundException("Venue", request.VenueId.Value);

            if (venue.OwnerId != ownerId)
                throw new UnauthorizedException("You can only create discounts for your own venues");
        }
        else if (request.Scope == "Court")
        {
            if (!request.CourtId.HasValue)
                throw new BusinessException("CourtId is required for court-scoped discounts");

            var court = await _courtRepository.GetByIdAsync(request.CourtId.Value);
            if (court == null)
                throw new NotFoundException("Court", request.CourtId.Value);

            var venue = await _venueRepository.GetByIdAsync(court.VenueId);
            if (venue == null || venue.OwnerId != ownerId)
                throw new UnauthorizedException("You can only create discounts for courts in your own venues");
        }

        var discount = new Discount
        {
            Scope = request.Scope,
            VenueId = request.VenueId,
            CourtId = request.CourtId,
            PercentOff = request.PercentOff,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _discountRepository.AddAsync(discount);
        await _discountRepository.SaveChangesAsync();

        return await MapToResponse(discount);
    }

    public async Task<IEnumerable<DiscountResponse>> GetMyDiscountsAsync(int ownerId)
    {
        var myVenues = await _venueRepository.GetVenuesByOwnerAsync(ownerId);
        var venueIds = myVenues.Select(v => v.VenueId).ToList();

        var discounts = await _discountRepository.FindAsync(d =>
            (d.VenueId.HasValue && venueIds.Contains(d.VenueId.Value)) ||
            (d.CourtId.HasValue && d.Court != null && venueIds.Contains(d.Court.VenueId)));

        return await Task.WhenAll(discounts.Select(MapToResponse));
    }

    public async Task<IEnumerable<DiscountResponse>> GetActiveDiscountsAsync()
    {
        var now = DateTime.UtcNow;
        var discounts = await _discountRepository.FindAsync(d =>
            d.IsActive &&
            d.ValidFrom <= now &&
            d.ValidTo >= now);

        return await Task.WhenAll(discounts.Select(MapToResponse));
    }

    public async Task<DiscountResponse?> GetApplicableDiscountAsync(int? venueId, int? courtId, DateTime slotTime)
    {
        var discounts = await _discountRepository.FindAsync(d =>
            d.IsActive &&
            d.ValidFrom <= slotTime &&
            d.ValidTo >= slotTime &&
            ((d.Scope == "Court" && d.CourtId == courtId) ||
             (d.Scope == "Venue" && d.VenueId == venueId)));

        var discount = discounts.OrderByDescending(d => d.PercentOff).FirstOrDefault();
        return discount != null ? await MapToResponse(discount) : null;
    }

    public async Task<IEnumerable<DiscountResponse>> GetDiscountsByVenueAsync(int venueId)
    {
        var discounts = await _discountRepository.FindAsync(d =>
            d.VenueId == venueId && d.IsActive);
        return await Task.WhenAll(discounts.Select(MapToResponse));
    }

    public async Task<IEnumerable<DiscountResponse>> GetDiscountsByCourtAsync(int courtId)
    {
        var discounts = await _discountRepository.FindAsync(d =>
            d.CourtId == courtId && d.IsActive);
        return await Task.WhenAll(discounts.Select(MapToResponse));
    }

    private async Task<DiscountResponse> MapToResponse(Discount discount)
    {
        string? venueName = null;
        string? courtName = null;

        if (discount.VenueId.HasValue)
        {
            var venue = await _venueRepository.GetByIdAsync(discount.VenueId.Value);
            venueName = venue?.Name;
        }

        if (discount.CourtId.HasValue)
        {
            var court = await _courtRepository.GetByIdAsync(discount.CourtId.Value);
            courtName = court?.Name;
        }

        return new DiscountResponse
        {
            DiscountId = discount.DiscountId,
            Scope = discount.Scope,
            VenueId = discount.VenueId,
            VenueName = venueName,
            CourtId = discount.CourtId,
            CourtName = courtName,
            PercentOff = discount.PercentOff,
            ValidFrom = discount.ValidFrom,
            ValidTo = discount.ValidTo,
            IsActive = discount.IsActive,
            CreatedAt = discount.CreatedAt
        };
    }
}
