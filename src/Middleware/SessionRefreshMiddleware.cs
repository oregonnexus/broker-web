using OregonNexus.Broker.Web.Services.Sessions;

namespace OregonNexus.Broker.Web.Middleware;

public class SessionRefreshMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ISessionRefresherService _sessionRefresherService;

    public SessionRefreshMiddleware(
        RequestDelegate next,
        ISessionRefresherService sessionRefresherService)
    {
        _next = next;
        _sessionRefresherService = sessionRefresherService;
    }

    public async Task Invoke(HttpContext context)
    {
        await _sessionRefresherService.RefreshSession(context);
        await _next(context);
    }
}
