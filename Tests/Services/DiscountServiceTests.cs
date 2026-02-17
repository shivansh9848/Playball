using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Application.Services;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Infrastructure.Repositories;

namespace Playball.Tests.Services;

public class DiscountServiceTests
{
    private readonly Mock<IRepository<Discount>> _discountRepoMock;
    private readonly Mock<IVenueRepository> _venueRepoMock;
    private readonly Mock<IRepository<Court>> _courtRepoMock;
    private readonly DiscountService _discountService;

    public DiscountServiceTests()
    {
        _discountRepoMock = new Mock<IRepository<Discount>>();
        _venueRepoMock = new Mock<IVenueRepository>();
        _courtRepoMock = new Mock<IRepository<Court>>();
        _discountService = new DiscountService(_discountRepoMock.Object, _venueRepoMock.Object, _courtRepoMock.Object);
    }

    [Fact]
    public async Task CreateDiscountAsync_ShouldCreate_VenueScoped()
    {
        var venue = new Venue { VenueId = 1, OwnerId = 1, Name = "Arena" };
        _venueRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(venue);

        var request = new CreateDiscountRequest
        {
            Scope = "Venue", VenueId = 1, PercentOff = 20,
            ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddDays(30)
        };

        var result = await _discountService.CreateDiscountAsync(1, request);

        Assert.Equal(20, result.PercentOff);
        _discountRepoMock.Verify(r => r.AddAsync(It.IsAny<Discount>()), Times.Once);
    }

    [Fact]
    public async Task CreateDiscountAsync_ShouldCreate_CourtScoped()
    {
        var court = new Court { CourtId = 1, VenueId = 1, Name = "Court A" };
        var venue = new Venue { VenueId = 1, OwnerId = 1, Name = "Arena" };
        _courtRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(court);
        _venueRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(venue);

        var request = new CreateDiscountRequest
        {
            Scope = "Court", CourtId = 1, PercentOff = 15,
            ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddDays(7)
        };

        var result = await _discountService.CreateDiscountAsync(1, request);

        Assert.Equal(15, result.PercentOff);
    }

    [Fact]
    public async Task CreateDiscountAsync_ShouldThrow_WhenDateRangeInvalid()
    {
        var request = new CreateDiscountRequest
        {
            Scope = "Venue", VenueId = 1, PercentOff = 20,
            ValidFrom = DateTime.UtcNow.AddDays(30), ValidTo = DateTime.UtcNow
        };

        await Assert.ThrowsAsync<BusinessException>(() => _discountService.CreateDiscountAsync(1, request));
    }

    [Fact]
    public async Task CreateDiscountAsync_ShouldThrow_WhenVenueScopedWithoutVenueId()
    {
        var request = new CreateDiscountRequest
        {
            Scope = "Venue", VenueId = null, PercentOff = 10,
            ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddDays(7)
        };

        await Assert.ThrowsAsync<BusinessException>(() => _discountService.CreateDiscountAsync(1, request));
    }

    [Fact]
    public async Task CreateDiscountAsync_ShouldThrow_WhenNotVenueOwner()
    {
        var venue = new Venue { VenueId = 1, OwnerId = 2, Name = "Arena" };
        _venueRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(venue);

        var request = new CreateDiscountRequest
        {
            Scope = "Venue", VenueId = 1, PercentOff = 20,
            ValidFrom = DateTime.UtcNow, ValidTo = DateTime.UtcNow.AddDays(7)
        };

        await Assert.ThrowsAsync<UnauthorizedException>(() => _discountService.CreateDiscountAsync(1, request));
    }

    [Fact]
    public async Task GetApplicableDiscountAsync_ShouldReturnBestDiscount()
    {
        var discounts = new List<Discount>
        {
            new() { DiscountId = 1, Scope = "Court", CourtId = 1, PercentOff = 10, IsActive = true },
            new() { DiscountId = 2, Scope = "Court", CourtId = 1, PercentOff = 20, IsActive = true }
        };
        _discountRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Discount, bool>>>())).ReturnsAsync(discounts);

        var result = await _discountService.GetApplicableDiscountAsync(1, 1, DateTime.UtcNow.AddDays(1));

        Assert.NotNull(result);
        Assert.Equal(20, result!.PercentOff);
    }

    [Fact]
    public async Task GetApplicableDiscountAsync_ShouldReturnNull_WhenNoActiveDiscounts()
    {
        _discountRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Discount, bool>>>()))
            .ReturnsAsync(new List<Discount>());

        var result = await _discountService.GetApplicableDiscountAsync(1, 1, DateTime.UtcNow);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveDiscountsAsync_ShouldReturnActiveDiscounts()
    {
        var discounts = new List<Discount>
        {
            new() { DiscountId = 1, Scope = "Venue", VenueId = 1, PercentOff = 10, IsActive = true }
        };
        _discountRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Discount, bool>>>())).ReturnsAsync(discounts);
        _venueRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Venue { Name = "Arena" });

        var result = await _discountService.GetActiveDiscountsAsync();

        Assert.Single(result);
    }
}
