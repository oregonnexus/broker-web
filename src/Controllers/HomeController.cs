// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OregonNexus.Broker.Web.Models;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.SharedKernel;
using OregonNexus.Broker.Web.ViewModels.OutgoingRequests;
using OregonNexus.Broker.Web.ViewModels.IncomingRequests;
using OregonNexus.Broker.Web.ViewModels.EducationOrganizations;
using OregonNexus.Broker.Web.Helpers;

namespace OregonNexus.Broker.Web.Controllers;

[Authorize]
public class HomeController : AuthenticatedController<HomeController>
{
    private readonly ILogger<HomeController> _logger;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<EducationOrganization> _educationOrganizationRepository;
    private readonly IRepository<Request> _requestRepository;
    private readonly FocusHelper _focusHelper;

    public HomeController(
        IHttpContextAccessor httpContextAccessor,
        IRepository<User> userRepository,
        IRepository<EducationOrganization> educationOrganizationRepository,
        IRepository<Request> requestRepository,
        ILogger<HomeController> logger,
        FocusHelper focusHelper)
    {
        _userRepository = userRepository;
        _educationOrganizationRepository = educationOrganizationRepository;
        _requestRepository = requestRepository;
        _logger = logger;
        _focusHelper = focusHelper;
    }

    public async Task<IActionResult> Index(
        DashboardViewModel model,
        CancellationToken cancellationToken)
    {
        // TODO: Refactor and optimize dashboard queries, using dirty temporary ops for mockup purposes.

        // IMPORTANT: Everything should be filtered by current Focus
        // Some queries can be omitted based on user role.

        // We will need to support a date range (start and end dates) to filter out the data.
        // Currently displaying last 7 days, last 30 days, and All-time.
        // To support All-time, the end date should be nullable.
        // For example, Outgoing Processor does not need to see incoming record request data.
        var educationOrganizationId = GetFocusOrganizationId();

        if (educationOrganizationId == Guid.Empty)
        {
            var focusableOrganizations = await _focusHelper.GetFocusableEducationOrganizationsSelectList();
            educationOrganizationId = focusableOrganizations.FirstOrDefault()?.Value is string valueStr && Guid.TryParse(valueStr, out var result)
                ? result
                : Guid.Empty;
        }

        var requests = (await _requestRepository.ListAsync(cancellationToken))
            .OrderByDescending(incomingRequest => incomingRequest.CreatedAt)
            .Where(request => model.StartDate is null || request.CreatedAt >= model.StartDate)
            .ToList();

        // Only take 5, displaying latest incoming requests
        // Need the total count as well
        var incomingRequests = requests;
            //.Where(request => request.ToEducationOrganizationId == educationOrganizationId);

        // Only take 5, displaying latest outgoing requests
        // Need the total count as well
        var outgoingRequests = requests
            .Where(request => request.EducationOrganizationId == educationOrganizationId
            && request.RequestStatus != RequestStatus.Draft);

        var usersCount = await _userRepository.CountAsync(cancellationToken);

        var educationOrganizations = (await _educationOrganizationRepository.ListAsync(cancellationToken))
            .Where(educationOrganization => educationOrganization.Id == educationOrganizationId)
            .OrderByDescending(organization => organization.CreatedAt)
            .Select(educationOrganization => new EducationOrganizationRequestViewModel(educationOrganization))
            .ToList();

        // Temporary, taking 5 here
        var incomingRequestViewModels = incomingRequests
            .Take(5)
            .Select(incomingRequest =>  new IncomingRequestViewModel(incomingRequest))
            .ToList();

        // Temporary, taking 5 here
        var outgoingRequestViewModels = outgoingRequests
            .Take(5)
            .Select(outgoingRequest => new OutgoingRequestViewModel(outgoingRequest))
            .ToList();


        model.InitialRequestsCount = incomingRequests.Count();
        model.OutgoingRequestsCount = outgoingRequests.Count();
        model.DraftCount = requests.Count(request => request.RequestStatus == RequestStatus.Draft);
        model.WaitingApprovalCount = requests.Count(request => request.RequestStatus == RequestStatus.WaitingApproval);
        model.ApprovedCount = requests.Count(request => request.RequestStatus == RequestStatus.Approved);
        model.DeclinedCount = requests.Count(request => request.RequestStatus == RequestStatus.Declined);
        model.EducationOrganizationsCount = educationOrganizations.Count;
        model.EducationOrganizations = educationOrganizations;
        model.UsersCount = usersCount;
        model.LatestIncomingRequests = incomingRequestViewModels;
        model.LatestOutgoingRequests = outgoingRequestViewModels;
        model.StartDate = model.StartDate;  

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
