// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using System.Diagnostics;
using InertiaAdapter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OregonNexus.Broker.Web.Models;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.SharedKernel;
using OregonNexus.Broker.Web.Extensions.RequestStatuses;

namespace OregonNexus.Broker.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<EducationOrganization> _educationOrganizationRepository;
    private readonly IRepository<IncomingRequest> _incomingRequestRepository;
    private readonly IRepository<OutgoingRequest> _outgoingRequestRepository;

    public HomeController(
        IRepository<User> userRepository,
        IRepository<EducationOrganization> educationOrganizationRepository,
        IRepository<IncomingRequest> incomingRequestRepository,
        IRepository<OutgoingRequest> outgoingRequestRepository,
        ILogger<HomeController> logger)
    {
        _userRepository = userRepository;
        _educationOrganizationRepository = educationOrganizationRepository;
        _incomingRequestRepository = incomingRequestRepository;
        _outgoingRequestRepository = outgoingRequestRepository;
        _logger = logger;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        // TODO: Refactor and optimize dashboard queries, using dirty temporary ops for mockup purposes.

        // IMPORTANT: Everything should be filtered by current Focus
        // Some queries can be omitted based on user role.

        // We will need to support a date range (start and end dates) to filter out the data.
        // Currently displaying last 7 days, last 30 days, and All-time.
        // To support All-time, the end date should be nullable.
        // For example, Outgoing Processor does not need to see incoming transfer request data.

        // Only take 5, displaying latest incoming requests
        // Need the total count as well
        var incomingRequests = (await _incomingRequestRepository.ListAsync(cancellationToken))
            .OrderByDescending(incomingRequest => incomingRequest.CreatedAt)
            .ToList();

        // Only take 5, displaying latest outgoing requests
        // Need the total count as well
        var outgoingRequests = (await _outgoingRequestRepository.ListAsync(cancellationToken))
            .OrderByDescending(outgoingRequest => outgoingRequest.CreatedAt)
            .ToList();

        var usersCount = await _userRepository.CountAsync(cancellationToken);

        var organizations = (await _educationOrganizationRepository.ListAsync(cancellationToken))
            .ToList();

        // Temporary, taking 5 here
        var incomingRequestViewModels = incomingRequests
            .Take(5)
            .Select(incomingRequest =>  new IncomingRequestViewModel()
            {
                Id = incomingRequest.Id,
                District = incomingRequest.EducationOrganization?.ParentOrganization?.Name ?? string.Empty,
                School = incomingRequest.EducationOrganization?.Name ?? string.Empty,
                Student = incomingRequest.Student,
                Date = incomingRequest.RequestDate,
                Status = incomingRequest.RequestStatus.ToFriendlyString()
            })
            .ToList();

        // Temporary, taking 5 here
        var outgoingRequestViewModels = outgoingRequests
            .Take(5)
            .Select(outgoingRequest =>  new OutgoingRequestViewModel()
            {
                Id = outgoingRequest.Id,
                District = outgoingRequest.EducationOrganization?.ParentOrganization?.Name ?? string.Empty,
                School = outgoingRequest.EducationOrganization?.Name ?? string.Empty,
                Student = outgoingRequest.Student,
                Date = outgoingRequest.RequestDate,
                Status = outgoingRequest.RequestStatus.ToFriendlyString()
            })
            .ToList();

        var tempData = new DashboardViewModel()
        {
            InitialRequestsCount = incomingRequests.Count,
            OutgoingRequestsCount = outgoingRequests.Count,
            OrganizationsCount = organizations.Count,
            UsersCount = usersCount,
            LatestIncomingRequests = incomingRequestViewModels,
            LatestOutgoingRequests = outgoingRequestViewModels,
        };
        return View(tempData);
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
