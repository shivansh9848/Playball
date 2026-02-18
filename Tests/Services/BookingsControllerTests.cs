using System.Threading.Tasks;
using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Controllers;
using Assignment_Example_HU.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Collections.Generic;

namespace Playball.Tests.Controllers
{
    public class BookingsControllerTests
    {
        private readonly Mock<IBookingService> _mockBookingService;
        private readonly BookingsController _controller;

        public BookingsControllerTests()
        {
            _mockBookingService = new Mock<IBookingService>();
            _controller = new BookingsController(_mockBookingService.Object);

            // Mock User
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "User")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task LockSlot_ShouldReturnOk_WhenServiceLocksSlot()
        {
            // Arrange
            var request = new LockSlotRequest { CourtId = 1, SlotStartTime = System.DateTime.UtcNow.AddHours(1) };
            var booking = new BookingResponse { BookingId = 123, PriceLocked = 500, Status = "Pending" };
            
            _mockBookingService.Setup(s => s.LockSlotAsync(1, request)).ReturnsAsync(booking);

            // Act
            var result = await _controller.LockSlot(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<BookingResponse>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(booking.BookingId, response.Data.BookingId);
        }

        [Fact]
        public async Task ConfirmBooking_ShouldReturnOk_WhenServiceConfirms()
        {
            // Arrange
            var request = new ConfirmBookingRequest { BookingId = 123 };
            var booking = new BookingResponse { BookingId = 123, Status = "Confirmed" };
            
            _mockBookingService.Setup(s => s.ConfirmBookingAsync(1, request)).ReturnsAsync(booking);

            // Act
            var result = await _controller.ConfirmBooking(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<BookingResponse>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Confirmed", response.Data.Status);
        }
    }
}
