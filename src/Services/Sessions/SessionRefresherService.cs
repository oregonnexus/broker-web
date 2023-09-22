using static OregonNexus.Broker.Web.Constants.Sessions.SessionKey;
namespace OregonNexus.Broker.Web.Services.Sessions;

public class SessionRefresherService : ISessionRefresherService
{
    public async Task RefreshSession(HttpContext context)
    {
        var session = context.Session;
        var userCurrentExists = session.Keys.Contains(UserCurrent);

        if (session != null
            && (context.User.Identity?.IsAuthenticated ?? false)
            && userCurrentExists)
        {
            context.Session.SetString(LastAccessedKey, DateTime.UtcNow.ToString());
        }
        else
        {
            if (!context.Request.Path.StartsWithSegments("/login/logout"))
            {
                context.Response.Redirect("/login/logout");
                await context.Response.CompleteAsync();
                return;
            }
        }
    }
}

