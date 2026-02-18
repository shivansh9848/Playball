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

namespace Playball.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AuthController(_mockAuthService.Object);
        }

        [Fact]
        public async Task Register_ShouldReturnOk_WhenServiceRegistersUser()
        {
            // Arrange
            var request = new RegisterRequest { Email = "test@example.com", Password = "Password123" };
            var response = new AuthResponse { Token = "jwt-token", UserId = 1, Role = "User" };
            
            _mockAuthService.Setup(s => s.RegisterAsync(request)).ReturnsAsync(response);

            // Act
            var result = await _controller.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<AuthResponse>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal("jwt-token", apiResponse.Data.Token);
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WhenServiceLogsInUser()
        {
            // Arrange
            var request = new LoginRequest { Email = "test@example.com", Password = "Password123" };
            var response = new AuthResponse { Token = "jwt-token", UserId = 1, Role = "User" };
            
            _mockAuthService.Setup(s => s.LoginAsync(request)).ReturnsAsync(response);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var apiResponse = Assert.IsType<ApiResponse<AuthResponse>>(okResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal("jwt-token", apiResponse.Data.Token);
        }
    }
}
