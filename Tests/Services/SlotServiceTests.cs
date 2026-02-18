using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Application.Services;
using Assignment_Example_HU.Common.Exceptions;
using Assignment_Example_HU.Domain.Entities;
using Assignment_Example_HU.Infrastructure.Repositories;
using Moq;
using Xunit;

namespace Playball.Tests.Services
{
    public class SlotServiceTests
    {
        private readonly Mock<IRepository<Court>> _mockCourtRepository;
        private readonly Mock<IBookingRepository> _mockBookingRepository;
        private readonly Mock<IPricingService> _mockPricingService;
        private readonly SlotService _slotService;

        public SlotServiceTests()
        {
            _mockCourtRepository = new Mock<IRepository<Court>>();
            _mockBookingRepository = new Mock<IBookingRepository>();
            _mockPricingService = new Mock<IPricingService>();

            _slotService = new SlotService(
                _mockCourtRepository.Object,
                _mockBookingRepository.Object,
                _mockPricingService.Object
            );
        }

        [Fact]
        public async Task GetAvailableSlotsAsync_ShouldReturnSlots_WhenCourtIsOpen()
        {
            // Arrange
            var courtId = 1;
            var date = DateTime.UtcNow.AddDays(1).Date; // Tomorrow
            var court = new Court 
            { 
                CourtId = courtId, 
                VenueId = 1, 
                Name = "Center Court", 
                OpenTime = "10:00", 
                CloseTime = "12:00", 
                SlotDurationMinutes = 60,
                BasePrice = 100
            };
            var pricing = new PricingBreakdown { FinalPrice = 120 };

            _mockCourtRepository.Setup(r => r.GetByIdAsync(courtId)).ReturnsAsync(court);
            _mockBookingRepository.Setup(r => r.IsSlotAvailableAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(true);
            _mockPricingService.Setup(p => p.GetPricingBreakdownAsync(courtId, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(pricing);

            // Act
            var result = await _slotService.GetAvailableSlotsAsync(courtId, date);

            // Assert
            Assert.NotNull(result);
            var slots = result.ToList();
            Assert.Equal(2, slots.Count); // 10-11, 11-12
            Assert.All(slots, s => Assert.True(s.IsAvailable));
            Assert.All(slots, s => Assert.Equal(pricing.FinalPrice, s.FinalPrice));
        }

        [Fact]
        public async Task GetSlotDetailsAsync_ShouldThrow_WhenSlotInPast()
        {
            // Arrange
            var courtId = 1;
            var pastTime = DateTime.UtcNow.AddHours(-1);
            var court = new Court { CourtId = courtId, OpenTime = "00:00", CloseTime = "23:59" };

            _mockCourtRepository.Setup(r => r.GetByIdAsync(courtId)).ReturnsAsync(court);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessException>(() => _slotService.GetSlotDetailsAsync(courtId, pastTime, pastTime.AddMinutes(60)));
        }

        [Fact]
        public async Task GetSlotDetailsAsync_ShouldReturnDetails_WhenValid()
        {
            // Arrange
            var courtId = 1;
            var startTime = DateTime.UtcNow.AddDays(1).Date.AddHours(14); // 2 PM tomorrow
            var endTime = startTime.AddHours(1);
            var court = new Court { CourtId = courtId, OpenTime = "10:00", CloseTime = "22:00", BasePrice = 100 };
            var pricing = new PricingBreakdown { FinalPrice = 150 };

            _mockCourtRepository.Setup(r => r.GetByIdAsync(courtId)).ReturnsAsync(court);
            _mockBookingRepository.Setup(r => r.IsSlotAvailableAsync(courtId, startTime, endTime)).ReturnsAsync(false); // Booked
            _mockPricingService.Setup(p => p.GetPricingBreakdownAsync(courtId, startTime, endTime)).ReturnsAsync(pricing);

            // Act
            var result = await _slotService.GetSlotDetailsAsync(courtId, startTime, endTime);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsAvailable);
            Assert.Equal(pricing.FinalPrice, result.FinalPrice);
        }
    }
}
