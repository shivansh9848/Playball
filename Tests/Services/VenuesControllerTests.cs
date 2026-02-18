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
using System.Collections.Generic;

namespace Playball.Tests.Controllers
{
    public class VenuesControllerTests
    {
        private readonly Mock<IVenueService> _mockVenueService;
        private readonly VenuesController _controller;

        public VenuesControllerTests()
        {
            _mockVenueService = new Mock<IVenueService>();
            _controller = new VenuesController(_mockVenueService.Object);

            // Mock User
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "VenueOwner")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task CreateVenue_ShouldReturnOk_WhenServiceCreatesVenue()
        {
            // Arrange
            var request = new CreateVenueRequest { Name = "Test Arena", Address = "123 St" };
            var venue = new VenueResponse { VenueId = 1, Name = "Test Arena" };
            
            _mockVenueService.Setup(s => s.CreateVenueAsync(1, request)).ReturnsAsync(venue);

            // Act
            var result = await _controller.CreateVenue(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<VenueResponse>(okResult.Value);
            Assert.Equal(venue.Name, response.Name);
        }

        [Fact]
        public async Task ApproveVenue_ShouldReturnOk_WhenAdminApproves()
        {
            // Arrange
            var request = new ApproveVenueRequest { ApprovalStatus = 2 }; // Approved
            var venue = new VenueResponse { VenueId = 1, Name = "Test Arena", ApprovalStatus = "Approved" };
            
            _mockVenueService.Setup(s => s.ApproveVenueAsync(1, 1, request)).ReturnsAsync(venue);

            // Mock Admin User
             var adminUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));
            _controller.ControllerContext.HttpContext.User = adminUser;

            // Act
            var result = await _controller.ApproveVenue(1, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<VenueResponse>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Approved", response.Data.ApprovalStatus);
        }
    }
}
