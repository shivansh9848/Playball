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
    public class GamesControllerTests
    {
        private readonly Mock<IGameService> _mockGameService;
        private readonly GamesController _controller;

        public GamesControllerTests()
        {
            _mockGameService = new Mock<IGameService>();
            _controller = new GamesController(_mockGameService.Object);

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
        public async Task CreateGame_ShouldReturnOk_WhenServiceCreatesGame()
        {
            // Arrange
            var request = new CreateGameRequest { Title = "Friendly Match", MinPlayers = 2, MaxPlayers = 4 };
            var game = new GameResponse { GameId = 1, Title = "Friendly Match", Status = "Open" };
            
            _mockGameService.Setup(s => s.CreateGameAsync(1, request)).ReturnsAsync(game);

            // Act
            var result = await _controller.CreateGame(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<GameResponse>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(game.Title, response.Data.Title);
        }

        [Fact]
        public async Task JoinGame_ShouldReturnOk_WhenServiceJoins()
        {
            // Arrange
            var gameId = 1;
            var game = new GameResponse { GameId = 1, Title = "Match", CurrentPlayers = 2, MaxPlayers = 4 };
            
            _mockGameService.Setup(s => s.JoinGameAsync(1, gameId)).ReturnsAsync(game);

            // Act
            var result = await _controller.JoinGame(gameId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<GameResponse>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(2, response.Data.CurrentPlayers);
        }

        [Fact]
        public async Task LeaveGame_ShouldReturnOk_WhenServiceLeaves()
        {
             // Arrange
            var gameId = 1;
            var game = new GameResponse { GameId = 1, Title = "Match", CurrentPlayers = 1 };
            
            _mockGameService.Setup(s => s.LeaveGameAsync(1, gameId)).ReturnsAsync(game);

            // Act
            var result = await _controller.LeaveGame(gameId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<GameResponse>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Contains("left", response.Message);
        }
    }
}
