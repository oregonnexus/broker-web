using EdNexusData.Broker.SharedKernel;

namespace EdNexusData.Broker.Web.Services;

public class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContext;

    public CurrentUserService(IHttpContextAccessor httpContext)
    {
        _httpContext = httpContext;
    }

    public Guid? AuthenticatedUserId()
    {
        var SessionUserId = _httpContext.HttpContext!.User.Claims.Where(v => v.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").FirstOrDefault()?.Value;
        if (Guid.TryParse(SessionUserId, out var sessionId))
        {
            return sessionId;
        }
        return null;
    }
}