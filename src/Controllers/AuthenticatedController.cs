using Microsoft.AspNetCore.Mvc;
using static OregonNexus.Broker.Web.Constants.Sessions.SessionKey;

namespace OregonNexus.Broker.Web.Controllers;

public class AuthenticatedController : Controller
{
    protected readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticatedController(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected void RefreshSession()
    {
        var context = _httpContextAccessor.HttpContext;
        context?.Session.SetString("LastAccessed", DateTime.UtcNow.ToString());
    }

    protected void ValidateSession()
    {
        var context = _httpContextAccessor.HttpContext;
        if(context is not null && !context.Request.Path.StartsWithSegments("/login/logout"))
        {
            context.Response.Redirect("/login/logout");
            return;
        }
    }

    protected Guid GetFocusOrganizationId()
    {
        var session = _httpContextAccessor?.HttpContext?.Session;
        _ = Guid.TryParse(session?.GetString(FocusOrganizationKey), out var focusOrganizationKey);
        return focusOrganizationKey;
    }


    protected string GetFocusOrganizationDistrict()
    {
        var session = _httpContextAccessor?.HttpContext?.Session;
        return session is null ? string.Empty : session.GetString(FocusOrganizationDistrict) ?? string.Empty;
    }
        protected string GetFocusOrganizationSchool()
    {
        var session = _httpContextAccessor?.HttpContext?.Session;
        return session is null ? string.Empty : session.GetString(FocusOrganizationSchool) ?? string.Empty;
    }
}
