using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Playball.Tests.Controllers
{
    public class SlotsControllerTests
    {
        private readonly Mock<ISlotService> _mockSlotService;
        private readonly SlotsController _controller;

        public SlotsControllerTests()
        {
            _mockSlotService = new Mock<ISlotService>();
            _controller = new SlotsController(_mockSlotService.Object);
        }

        [Fact]
        public async Task GetAvailableSlots_ShouldReturnOk_WhenServiceReturnsSlots()
        {
            // Arrange
            var courtId = 1;
            var date = DateTime.UtcNow.Date.AddDays(1);
            var slots = new List<SlotAvailabilityResponse>
            {
                new SlotAvailabilityResponse { CourtId = 1, SlotStartTime = date.AddHours(10), IsAvailable = true }
            };
            
            _mockSlotService.Setup(s => s.GetAvailableSlotsAsync(courtId, date)).ReturnsAsync(slots);

            // Act
            var result = await _controller.GetAvailableSlots(courtId, date);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedSlots = Assert.IsType<List<SlotAvailabilityResponse>>(okResult.Value);
            Assert.Single(returnedSlots);
        }

        [Fact]
        public async Task GetSlotDetails_ShouldReturnOk_WhenServiceReturnsDetails()
        {
            // Arrange
            var courtId = 1;
            var start = DateTime.UtcNow.AddHours(1);
            var end = start.AddHours(1);
            var slot = new SlotAvailabilityResponse { CourtId = 1, SlotStartTime = start, FinalPrice = 100 };
            
            _mockSlotService.Setup(s => s.GetSlotDetailsAsync(courtId, start, end)).ReturnsAsync(slot);

            // Act
            var result = await _controller.GetSlotDetails(courtId, start, end);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<SlotAvailabilityResponse>(okResult.Value);
            Assert.Equal(100, response.FinalPrice);
        }
    }
}
