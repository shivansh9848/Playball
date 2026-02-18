using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignment_Example_HU.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminAuthService _adminAuthService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminAuthService adminAuthService, ILogger<AdminController> logger)
    {
        _adminAuthService = adminAuthService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new Admin account (requires admin secret key)
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegisterAdmin(
        [FromBody] RegisterUserRequest request,
        [FromHeader(Name = "X-Admin-Secret")] string adminSecretKey)
    {
        try
        {
            var response = await _adminAuthService.RegisterAdminAsync(request, adminSecretKey);
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Admin registered successfully"));
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (BusinessException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during admin registration");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during admin registration"));
        }
    }

    /// <summary>
    /// Login as Admin (only Admin role accounts can use this endpoint)
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginAdmin([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _adminAuthService.LoginAdminAsync(request);
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(response, "Admin login successful"));
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during admin login");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during admin login"));
        }
    }
}
