using Assignment_Example_HU.Application.DTOs.Request;
using Assignment_Example_HU.Application.Interfaces;
using Assignment_Example_HU.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assignment_Example_HU.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiscountsController : ControllerBase
{
    private readonly IDiscountService _discountService;

    public DiscountsController(IDiscountService discountService)
    {
        _discountService = discountService;
    }

    [HttpPost]
    [Authorize(Roles = "VenueOwner")]
    public async Task<IActionResult> CreateDiscount([FromBody] CreateDiscountRequest request)
    {
        var discount = await _discountService.CreateDiscountAsync(User.GetUserId(), request);
        return Ok(discount);
    }

    [HttpGet]
    [Authorize(Roles = "VenueOwner,Admin")]
    public async Task<IActionResult> GetMyDiscounts()
    {
        var discounts = await _discountService.GetMyDiscountsAsync(User.GetUserId());
        return Ok(discounts);
    }

    [HttpGet("venue/{venueId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDiscountsByVenue(int venueId)
    {
        var discounts = await _discountService.GetDiscountsByVenueAsync(venueId);
        return Ok(discounts);
    }

    [HttpGet("court/{courtId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDiscountsByCourt(int courtId)
    {
        var discounts = await _discountService.GetDiscountsByCourtAsync(courtId);
        return Ok(discounts);
    }
}
