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
using OregonNexus.Broker.Web.Models.JsonDocuments;
using OregonNexus.Broker.Web.Services.PayloadContents;
using OregonNexus.Broker.Web.MapperExtensions.JsonDocuments;
using static OregonNexus.Broker.Web.Constants.Claims.CustomClaimType;
using src.Models.Students;
namespace OregonNexus.Broker.Web.Controllers;

[Authorize(Policy = TransferOutgoingRecords)]
public class OutgoingController : AuthenticatedController
{
    private readonly IRepository<Request> _outgoingRequestRepository;
    private readonly IRepository<PayloadContent> _payloadContentRepository;
    private readonly IRepository<EducationOrganization> _educationOrganizationRepository;
    private readonly IPayloadContentService _payloadContentService;


    public OutgoingController(
        IHttpContextAccessor httpContextAccessor,
        IRepository<Request> outgoingRequestRepository,
        IRepository<PayloadContent> payloadContentRepository,
        IRepository<EducationOrganization> educationOrganizationRepository,
        IPayloadContentService payloadContentService) : base(httpContextAccessor)
    {
        _outgoingRequestRepository = outgoingRequestRepository;
        _payloadContentRepository = payloadContentRepository;
        _educationOrganizationRepository = educationOrganizationRepository;
        _payloadContentService = payloadContentService;
    }

    public async Task<IActionResult> Index(
      OutgoingRequestModel model,
      CancellationToken cancellationToken)
    {
        RefreshSession();

        var searchExpressions = model.BuildSearchExpressions(GetFocusOrganizationId());
        var sortExpression = model.BuildSortExpression();
        var organizationName = GetFocusOrganizationDistrict();

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

        outgoingRequestViewModels = outgoingRequestViewModels.Where(request => request.ReleasingDistrict == organizationName);
        totalItems = outgoingRequestViewModels.Count();

        if (!string.IsNullOrWhiteSpace(model.SearchBy))
        {
            outgoingRequestViewModels = outgoingRequestViewModels
                .Where(request => request.Student?.ToLower().Contains(model.SearchBy) is true || request.District.ToLower().Contains(model.SearchBy)
                 || request.School.ToLower().Contains(model.SearchBy));
            totalItems = outgoingRequestViewModels.Count();
        }

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

    public async Task<IActionResult> Update(Guid requestId)
    {
        var outgoingRequest = await _outgoingRequestRepository.GetByIdAsync(requestId);
        if (outgoingRequest is null) return NotFound();

        var educationOrganizations = await _educationOrganizationRepository.ListAsync();

        var edfiStudent = outgoingRequest.Student?.DeserializeFromJsonDocument<EdfiJsonModel>();

        var responseManifest = outgoingRequest.ResponseManifest?.DeserializeFromJsonDocument<ResponseManifestJsonModel>();

        var viewModel = new CreateOutgoingRequestViewModel
        {
            Id = responseManifest?.Student?.Id,
            EducationOrganizations = educationOrganizations,
            EducationOrganizationId = outgoingRequest.EducationOrganizationId,
            RequestId = outgoingRequest.Id,
            Date = outgoingRequest.CreatedAt,
            EdfiId = edfiStudent?.Student?.Id,
            EdfiStudentUniqueId = responseManifest?.Student?.StudentUniqueId,
            StudentUniqueId = responseManifest?.Student?.StudentUniqueId,
            FirstName = responseManifest?.Student?.FirstName,
            MiddleName = responseManifest?.Student?.MiddleName,
            LastSurname = responseManifest?.Student?.LastSurname,
            FromDistrict = responseManifest?.From?.District,
            FromSchool = responseManifest?.From?.School,
            FromEmail = responseManifest?.From?.Email,
            ToDistrict = responseManifest?.To?.District,
            ToSchool = responseManifest?.To?.School,
            ToEmail = responseManifest?.To?.Email,
            Note = responseManifest?.Note,
            Contents = responseManifest?.Contents,
            RequestStatus = outgoingRequest.RequestStatus
        };

        return View(viewModel);
    }

    [HttpPut]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(CreateOutgoingRequestViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var userId = Guid.Parse(User.FindFirstValue(claimType: ClaimTypes.NameIdentifier)!);

            var outgoingRequest = await _outgoingRequestRepository.GetByIdAsync(viewModel.RequestId);

            if (outgoingRequest is null) return BadRequest();

            var edfiStudentModel = viewModel.MapToEdfiStudentJsonModel();
            var responseManifest = viewModel.MapToResponseManifestJsonModel();

            var today = DateTime.UtcNow;

            outgoingRequest.EducationOrganizationId = viewModel.EducationOrganizationId;
            outgoingRequest.Student = edfiStudentModel;
            outgoingRequest.ResponseManifest = responseManifest;
            outgoingRequest.RequestStatus = viewModel.RequestStatus;
            outgoingRequest.UpdatedAt = today;
            outgoingRequest.UpdatedBy = userId;

            await _outgoingRequestRepository.UpdateAsync(outgoingRequest);
            await _outgoingRequestRepository.SaveChangesAsync();

            await _payloadContentService.AddPayloadContentsAsync(viewModel.Files, viewModel.RequestId);

            return RedirectToAction(nameof(Index));
        }

        var educationOrganizations = await _educationOrganizationRepository.ListAsync();
        viewModel.EducationOrganizations = educationOrganizations;
        return View(viewModel);
    }

}
