// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.SharedKernel;
using OregonNexus.Broker.Web.Extensions.RequestStatuses;
using OregonNexus.Broker.Web.Models;
using Microsoft.AspNetCore.Authorization;
using InertiaAdapter;

namespace OregonNexus.Broker.Web.Controllers;

[Authorize(Policy = "TransferRecords")]
public class OutgoingController : Controller
{
    private readonly IRepository<OutgoingRequest> _outgoingRequestRepository;

    public OutgoingController(
        IRepository<OutgoingRequest> outgoingRequestRepository)
    {
        _outgoingRequestRepository = outgoingRequestRepository;
    }
    
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var outgoingRequests = (await _outgoingRequestRepository.ListAsync(cancellationToken))
            .OrderByDescending(outgoingRequest => outgoingRequest.CreatedAt)
            .ToList();

        var outgoingRequestViewModels = outgoingRequests
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

        return View(outgoingRequestViewModels);
    }
}
