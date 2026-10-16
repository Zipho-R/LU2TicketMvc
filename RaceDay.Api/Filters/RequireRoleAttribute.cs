using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Api.Filters;

/// <summary>
/// Requires the caller to be logged in (a UserId must exist in the session).
/// If one or more roles are supplied, the logged-in user's Role must match
/// one of them, otherwise a 403 Forbidden is returned.
/// Usage: [RequireRole] for "must be logged in", [RequireRole("Organiser")]
/// for "must be logged in AND be an Organiser".
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RequireRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _allowedRoles;

    public RequireRoleAttribute(params string[] allowedRoles)
    {
        _allowedRoles = allowedRoles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var session = context.HttpContext.Session;
        var userId = session.GetInt32(SessionKeys.UserId);

        if (userId is null)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "You must be logged in." });
            return;
        }

        if (_allowedRoles.Length == 0)
        {
            // Any authenticated user is allowed.
            return;
        }

        var role = session.GetString(SessionKeys.Role);
        if (role is null || !_allowedRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
        {
            context.Result = new ObjectResult(new { message = "You do not have permission to perform this action." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
