using System.Security.Claims;

namespace EduVoice.API.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var schoolIdClaim = context.User.FindFirstValue("schoolId");

            if (!string.IsNullOrEmpty(schoolIdClaim) && Guid.TryParse(schoolIdClaim, out var schoolId))
            {
                context.Items["SchoolId"] = schoolId.ToString();
                _logger.LogDebug("Tenant context set: SchoolId={SchoolId}", schoolId);
            }

            var userIdClaim = context.User.FindFirstValue("userId");
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
            {
                context.Items["UserId"] = userId.ToString();
            }

            var roleClaim = context.User.FindFirstValue(ClaimTypes.Role)
                ?? context.User.FindFirstValue("role");
            if (!string.IsNullOrEmpty(roleClaim))
            {
                context.Items["UserRole"] = roleClaim;
            }
        }

        await _next(context);
    }
}
