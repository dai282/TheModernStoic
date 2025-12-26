
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
            var id = _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return id ?? throw new UnauthorizedAccessException("User is not authenticated.");
        }
    }
}