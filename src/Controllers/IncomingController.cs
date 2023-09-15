// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Mvc;
using OregonNexus.Broker.Web.Models;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using OregonNexus.Broker.Data;
using OregonNexus.Broker.Domain.Specifications;

namespace OregonNexus.Broker.Web.Controllers;

[Authorize(Policy = "TransferRecords")]
public class IncomingController : Controller
{
    private readonly BrokerDbContext _db;
    private readonly IRepository<IncomingRequest> _incomingRequestRepository;

    public IncomingController(
        BrokerDbContext db,
        IRepository<IncomingRequest> incomingRequestRepository)
    {
        _db = db;
        _incomingRequestRepository = incomingRequestRepository;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var incomingRequests = (await _incomingRequestRepository.ListAsync(cancellationToken))
            .OrderByDescending(incomingRequest => incomingRequest.CreatedAt)
            .ToList();

        var incomingRequestViewModels = incomingRequests
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

        return View(incomingRequestViewModels);
    }
}
