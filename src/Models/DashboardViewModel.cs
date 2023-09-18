using System.ComponentModel.DataAnnotations;
using OregonNexus.Broker.Web.ViewModels.IncomingRequests;
using OregonNexus.Broker.Web.ViewModels.OutgoingRequests;

namespace OregonNexus.Broker.Web.Models;

public class DashboardViewModel
{
    [Display(Name = "Initial Requests Count")]
    public int InitialRequestsCount { get; set; }

    [Display(Name = "Outgoing Requests Count")]
    public int OutgoingRequestsCount { get; set; }

    [Display(Name = "Organizations Count")]
    public int OrganizationsCount { get; set; }

    [Display(Name = "Users Count")]
    public int UsersCount { get; set; }

    [Display(Name = "Latest Incoming Requests")]
    public IEnumerable<IncomingRequestViewModel> LatestIncomingRequests { get; set; } = Enumerable.Empty<IncomingRequestViewModel>();

    [Display(Name = "Latest Outgoing Requests")]
    public IEnumerable<OutgoingRequestViewModel> LatestOutgoingRequests { get; set; } = Enumerable.Empty<OutgoingRequestViewModel>();
}