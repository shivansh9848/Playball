using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignment_Example_HU.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly ILogger<WalletController> _logger;

    public WalletController(IWalletService walletService, ILogger<WalletController> logger)
    {
        _walletService = walletService;
        _logger = logger;
    }

    /// <summary>
    /// Add funds to user wallet (mock payment gateway)
    /// </summary>
    [HttpPost("add-funds")]
    [Authorize(Roles = "User,VenueOwner,GameOwner,Admin")]
    [ProducesResponseType(typeof(ApiResponse<WalletResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddFunds([FromBody] AddFundsRequest request)
    {
        try
        {
            var wallet = await _walletService.AddFundsAsync(User.GetUserId(), request.Amount, request.IdempotencyKey);
            return Ok(ApiResponse<WalletResponse>.SuccessResponse(wallet, "Funds added successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding funds");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while adding funds"));
        }
    }

    /// <summary>
    /// Get wallet balance for current user
    /// </summary>
    [HttpGet("balance")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<WalletResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalance()
    {
        try
        {
            var wallet = await _walletService.GetWalletByUserIdAsync(User.GetUserId());
            return Ok(ApiResponse<WalletResponse>.SuccessResponse(wallet, "Balance retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving balance");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving balance"));
        }
    }

    /// <summary>
    /// Get transaction history (Admin can view all, User can view own)
    /// </summary>
    [HttpGet("transactions")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<WalletTransactionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int? userId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var requestingUserId = User.GetUserId();
            var userRole = User.GetUserRole();

            // If userId is specified and user is not admin, ensure they can only view their own
            if (userId.HasValue && userId.Value != requestingUserId && userRole != "Admin")
            {
                return Forbid();
            }

            var targetUserId = userId ?? requestingUserId;
            var transactions = await _walletService.GetTransactionHistoryAsync(targetUserId, page, pageSize);
            
            return Ok(ApiResponse<IEnumerable<WalletTransactionResponse>>.SuccessResponse(
                transactions, 
                "Transactions retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving transactions"));
        }
    }
}
