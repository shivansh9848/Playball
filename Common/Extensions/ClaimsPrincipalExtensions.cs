using System.Security.Claims;

namespace Assignment_Example_HU.Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)
            ?? user.FindFirst("userId")
            ?? user.FindFirst("sub");

        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token");

        return userId;
    }

    public static string GetUserRole(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Role)?.Value
            ?? user.FindFirst("role")?.Value
            ?? throw new UnauthorizedAccessException("User role not found in token");
    }
}
