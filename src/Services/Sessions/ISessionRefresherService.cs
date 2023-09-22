namespace OregonNexus.Broker.Web.Services.Sessions;

public interface ISessionRefresherService
{
   Task RefreshSession(HttpContext context);
}