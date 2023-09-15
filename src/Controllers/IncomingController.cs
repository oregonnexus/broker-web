// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Web.Models;
using System.Security.Claims;
using OregonNexus.Broker.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using OregonNexus.Broker.Data;
using OregonNexus.Broker.Web.Extensions.RequestStatuses;

namespace OregonNexus.Broker.Web.Controllers;

[Authorize(Policy = "TransferRecords")]
public class IncomingController : Controller
{
    private readonly IRepository<EducationOrganization> _educationOrganizationRepository;
    private readonly IRepository<IncomingRequest> _incomingRequestRepository;

    public IncomingController(
        IRepository<EducationOrganization> educationOrganizationRepository,
        IRepository<IncomingRequest> incomingRequestRepository)
    {
        _educationOrganizationRepository = educationOrganizationRepository;
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

    public async Task<IActionResult> Create()
    {
        var educationOrganizations = await _educationOrganizationRepository.ListAsync();
        var viewModel = new CreateIncomingRequestViewModel
        {
            EducationOrganizations = educationOrganizations
        };

        return View(viewModel);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateIncomingRequestViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var userId = User.FindFirstValue(claimType: ClaimTypes.NameIdentifier)!;

            var incomingRequest = new IncomingRequest
            {
                EducationOrganizationId = viewModel.EducationOrganizationId,
                Student = viewModel.Student,
                RequestStatus = viewModel.RequestStatus,
                RequestUserId = Guid.Parse(userId),
                RequestDate = DateTime.UtcNow
            };

           await _incomingRequestRepository.AddAsync(incomingRequest);
           await _incomingRequestRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        var educationOrganizations = await _educationOrganizationRepository.ListAsync();
        viewModel.EducationOrganizations = educationOrganizations;
        return View(viewModel);
    }
}
