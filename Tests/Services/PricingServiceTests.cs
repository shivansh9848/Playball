using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Application.Services;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Constants;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Infrastructure.Caching;
using Assignment_Example_HU.Infrastructure.Repositories;
using Assignment_Example_HU.Common.Helpers;

namespace Playball.Tests.Services;

public class PricingServiceTests
{
    private readonly Mock<IRepository<Court>> _courtRepoMock;
    private readonly Mock<IDiscountService> _discountServiceMock;
    private readonly Mock<IBookingRepository> _bookingRepoMock;
    private readonly Mock<IRepository<Rating>> _ratingRepoMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly PricingService _pricingService;

    public PricingServiceTests()
    {
        _courtRepoMock = new Mock<IRepository<Court>>();
        _discountServiceMock = new Mock<IDiscountService>();
        _bookingRepoMock = new Mock<IBookingRepository>();
        _ratingRepoMock = new Mock<IRepository<Rating>>();
        _cacheServiceMock = new Mock<ICacheService>();
        _pricingService = new PricingService(
            _courtRepoMock.Object, _discountServiceMock.Object,
            _bookingRepoMock.Object, _ratingRepoMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task GetPricingBreakdownAsync_ShouldReturnBasePriceWithNoMultipliers()
    {
        var court = new Court { CourtId = 1, VenueId = 1, BasePrice = 100m };
        _courtRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(court);
        _cacheServiceMock.Setup(c => c.GetAsync<long>(It.IsAny<string>())).ReturnsAsync(0L);
        _ratingRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Rating, bool>>>()))
            .ReturnsAsync(new List<Rating>());
        _discountServiceMock.Setup(d => d.GetApplicableDiscountAsync(1, 1, It.IsAny<DateTime>()))
            .ReturnsAsync((DiscountResponse?)null);

        var slotStart = IstClock.Now.AddHours(48);
        var slotEnd = slotStart.AddHours(1);
        var result = await _pricingService.GetPricingBreakdownAsync(1, slotStart, slotEnd);

        Assert.Equal(100m, result.BasePrice);
        Assert.Equal(PricingConstants.DemandMultiplier_NoViewers, result.DemandMultiplier);
        Assert.Equal(PricingConstants.TimeMultiplier_MoreThan24Hours, result.TimeMultiplier);
        Assert.Equal(PricingConstants.HistoricalMultiplier_Low, result.HistoricalMultiplier);
    }

    [Fact]
    public async Task GetPricingBreakdownAsync_ShouldThrow_WhenCourtNotFound()
    {
        _courtRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Court?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _pricingService.GetPricingBreakdownAsync(99, IstClock.Now.AddHours(2), IstClock.Now.AddHours(3)));
    }

    [Fact]
    public async Task GetPricingBreakdownAsync_ShouldApplyDiscount()
    {
        var court = new Court { CourtId = 1, VenueId = 1, BasePrice = 100m };
        _courtRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(court);
        _cacheServiceMock.Setup(c => c.GetAsync<long>(It.IsAny<string>())).ReturnsAsync(0L);
        _ratingRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Rating, bool>>>()))
            .ReturnsAsync(new List<Rating>());
        _discountServiceMock.Setup(d => d.GetApplicableDiscountAsync(1, 1, It.IsAny<DateTime>()))
            .ReturnsAsync(new DiscountResponse { PercentOff = 20 });

        var slotStart = IstClock.Now.AddHours(48);
        var result = await _pricingService.GetPricingBreakdownAsync(1, slotStart, slotStart.AddHours(1));

        Assert.Equal(0.8m, result.DiscountFactor);
        Assert.True(result.DiscountAmount > 0);
        Assert.True(result.FinalPrice < 100m);
    }

    [Fact]
    public async Task GetPricingBreakdownAsync_ShouldApplyHighDemandMultiplier()
    {
        var court = new Court { CourtId = 1, VenueId = 1, BasePrice = 100m };
        _courtRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(court);
        _cacheServiceMock.Setup(c => c.GetAsync<long>(It.IsAny<string>())).ReturnsAsync(10L); // High demand
        _ratingRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Rating, bool>>>()))
            .ReturnsAsync(new List<Rating>());
        _discountServiceMock.Setup(d => d.GetApplicableDiscountAsync(1, 1, It.IsAny<DateTime>()))
            .ReturnsAsync((DiscountResponse?)null);

        var slotStart = IstClock.Now.AddHours(48);
        var result = await _pricingService.GetPricingBreakdownAsync(1, slotStart, slotStart.AddHours(1));

        Assert.Equal(PricingConstants.DemandMultiplier_MoreThan5Viewers, result.DemandMultiplier);
    }

    [Fact]
    public async Task GetPricingBreakdownAsync_ShouldApplyHighHistoricalMultiplier_WhenHighRatings()
    {
        var court = new Court { CourtId = 1, VenueId = 1, BasePrice = 100m };
        _courtRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(court);
        _cacheServiceMock.Setup(c => c.GetAsync<long>(It.IsAny<string>())).ReturnsAsync(0L);
        _ratingRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Rating, bool>>>()))
            .ReturnsAsync(new List<Rating> { new() { Score = 5 }, new() { Score = 4 }, new() { Score = 5 } });
        _discountServiceMock.Setup(d => d.GetApplicableDiscountAsync(1, 1, It.IsAny<DateTime>()))
            .ReturnsAsync((DiscountResponse?)null);

        var slotStart = IstClock.Now.AddHours(48);
        var result = await _pricingService.GetPricingBreakdownAsync(1, slotStart, slotStart.AddHours(1));

        Assert.Equal(PricingConstants.HistoricalMultiplier_High, result.HistoricalMultiplier);
    }

    [Fact]
    public async Task CalculateDynamicPriceAsync_ShouldReturnFinalPrice()
    {
        var court = new Court { CourtId = 1, VenueId = 1, BasePrice = 200m };
        _courtRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(court);
        _cacheServiceMock.Setup(c => c.GetAsync<long>(It.IsAny<string>())).ReturnsAsync(0L);
        _ratingRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Rating, bool>>>()))
            .ReturnsAsync(new List<Rating>());
        _discountServiceMock.Setup(d => d.GetApplicableDiscountAsync(1, 1, It.IsAny<DateTime>()))
            .ReturnsAsync((DiscountResponse?)null);

        var slotStart = IstClock.Now.AddHours(48);
        var price = await _pricingService.CalculateDynamicPriceAsync(1, slotStart, slotStart.AddHours(1));

        Assert.Equal(200m, price);
    }

    [Fact]
    public async Task TrackSlotViewAsync_ShouldIncrementCache()
    {
        _cacheServiceMock.Setup(c => c.GetAsync<long>(It.IsAny<string>())).ReturnsAsync(1L);

        await _pricingService.TrackSlotViewAsync(1, IstClock.Now.AddHours(2));

        _cacheServiceMock.Verify(c => c.IncrementAsync(It.IsAny<string>(), 1), Times.Once);
    }

    [Fact]
    public async Task GetSlotViewCountAsync_ShouldReturnCount()
    {
        _cacheServiceMock.Setup(c => c.GetAsync<long>(It.IsAny<string>())).ReturnsAsync(42L);

        var count = await _pricingService.GetSlotViewCountAsync(1, IstClock.Now.AddHours(2));

        Assert.Equal(42L, count);
    }
}
