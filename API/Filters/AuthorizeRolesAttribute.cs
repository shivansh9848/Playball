using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Assignment_Example_HU.Domain.Enums;
using System.Security.Claims;

namespace Assignment_Example_HU.API.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeRolesAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly UserRole[] _roles;

    public AuthorizeRolesAttribute(params UserRole[] roles)
    {
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Skip authorization if AllowAnonymous is present
        if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
        {
            return;
        }

        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (_roles.Length == 0)
        {
            return; // No specific roles required, just authentication
        }

        var userRole = user.FindFirst("Role")?.Value ?? user.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(userRole))
        {
            context.Result = new ForbidResult();
            return;
        }

        if (Enum.TryParse<UserRole>(userRole, out var role) && !_roles.Contains(role))
        {
            context.Result = new ForbidResult();
        }
    }
}
