
using System.Security.Claims;
using TheModernStoic.Application.Interfaces;

namespace TheModernStoic.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var id = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 2. Fallback: Try Auth0 specific "sub" claim
            if (string.IsNullOrEmpty(id))
            {
                id = user?.FindFirst("sub")?.Value;
            }

            return id ?? throw new UnauthorizedAccessException("User is not authenticated.");
        }
    }
}