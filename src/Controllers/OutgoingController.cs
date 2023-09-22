// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Mvc;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using OregonNexus.Broker.Web.Models.OutgoingRequests;
using OregonNexus.Broker.Web.Models.Paginations;
using OregonNexus.Broker.Web.Specifications.Paginations;
using System.Security.Claims;
using Ardalis.Specification;
using OregonNexus.Broker.Web.ViewModels.OutgoingRequests;

namespace OregonNexus.Broker.Web.Controllers;

[Authorize(Policy = "TransferRecords")]
public class OutgoingController : Controller
{
    private readonly IRepository<Request> _outgoingRequestRepository;
    private readonly IRepository<EducationOrganization> _educationOrganizationRepository;

    public OutgoingController(
        IRepository<Request> outgoingRequestRepository,
        IRepository<EducationOrganization> educationOrganizationRepository)
    {
        _outgoingRequestRepository = outgoingRequestRepository;
        _educationOrganizationRepository = educationOrganizationRepository;
    }

    public async Task<IActionResult> Index(
      OutgoingRequestModel model,
      CancellationToken cancellationToken)
    {
        var searchExpressions = model.BuildSearchExpressions();

        var sortExpression = model.BuildSortExpression();

        var specification = new SearchableWithPaginationSpecification<Request>.Builder(model.Page, model.Size)
            .WithAscending(model.IsAscending)
            .WithSortExpression(sortExpression)
            .WithSearchExpressions(searchExpressions)
            .WithIncludeEntities(builder => builder
                .Include(outgoingRequest => outgoingRequest.EducationOrganization)
                .ThenInclude(educationOrganization => educationOrganization!.ParentOrganization)
                )
            .Build();

        var totalItems = await _outgoingRequestRepository.CountAsync(
            specification,
            cancellationToken);

        var outgoingRequests = await _outgoingRequestRepository.ListAsync(
            specification,
            cancellationToken);

        var outgoingRequestViewModels = outgoingRequests
            .Select(outgoingRequest => new OutgoingRequestViewModel(outgoingRequest));

        var result = new PaginatedViewModel<OutgoingRequestViewModel>(
            outgoingRequestViewModels,
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
        var viewModel = new CreateOutgoingRequestViewModel
        {
            EducationOrganizations = educationOrganizations
        };

        return View(viewModel);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOutgoingRequestViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var userId = User.FindFirstValue(claimType: ClaimTypes.NameIdentifier)!;

            var outgoingRequest = new Request
            {
                EducationOrganizationId = viewModel.EducationOrganizationId,
                Student = null,//todo: create json document.
                RequestStatus = viewModel.RequestStatus,
                CreatedBy = Guid.Parse(userId),
                CreatedAt = DateTime.UtcNow
            };

            await _outgoingRequestRepository.AddAsync(outgoingRequest);
            await _outgoingRequestRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        var educationOrganizations = await _educationOrganizationRepository.ListAsync();
        viewModel.EducationOrganizations = educationOrganizations;
        return View(viewModel);
    }

}
