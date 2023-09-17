// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Web.Models;
using System.Security.Claims;
using OregonNexus.Broker.SharedKernel;
using OregonNexus.Broker.Web.Models.Paginations;
using Ardalis.Specification;
using OregonNexus.Broker.Web.Specifications.Paginations;
using OregonNexus.Broker.Web.Models.IncomingRequests;

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

    public async Task<IActionResult> Index(
        IncomingRequestModel model,
        CancellationToken cancellationToken)
    {
        var searchExpressions = model.BuildSearchExpressions();

        var sortExpression = model.BuildSortExpression();

        var specification = new SearchableWithPaginationSpecification<IncomingRequest>.Builder(model.Page, model.Size)
            .WithAscending(model.IsAscending)
            .WithSortExpression(sortExpression)
            .WithSearchExpressions(searchExpressions)
            .WithIncludeEntities(builder => builder
                .Include(incomingRequest => incomingRequest.EducationOrganization)
                .ThenInclude(educationOrganization => educationOrganization!.ParentOrganization)
                )
            .Build();

        var totalItems = await _incomingRequestRepository.CountAsync(cancellationToken);

        var incomingRequests = await _incomingRequestRepository.ListAsync(
            specification,
            cancellationToken);

        var incomingRequestViewModels = incomingRequests
            .Select(incomingRequest => new IncomingRequestViewModel(incomingRequest));

        var result = new PaginatedViewModel<IncomingRequestViewModel>(
            incomingRequestViewModels,
            totalItems,
            model.Page,
            model.Size,
            model.SortBy,
            model.SortDir,
            model.SearchBy);

        return View(result);
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
