using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Exceptions;
using System.Security.Claims;

namespace Assignment_Example_HU.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "User registered successfully"));
        }
        catch (BusinessException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during registration"));
        }
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Login successful"));
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during login"));
        }
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid token"));
            }

            var profile = await _authService.GetUserProfileAsync(userId);
            if (profile == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("User profile not found"));
            }

            return Ok(ApiResponse<UserProfileResponse>.SuccessResponse(profile, "Profile retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving profile"));
        }
    }

    /// <summary>
    /// Get user profile by ID (accessible by any authenticated user)
    /// </summary>
    [HttpGet("users/{id}/profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserProfile(int id)
    {
        try
        {
            var profile = await _authService.GetUserProfileAsync(id);
            if (profile == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
            }

            return Ok(ApiResponse<UserProfileResponse>.SuccessResponse(profile, "User profile retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving user profile"));
        }
    }
}
