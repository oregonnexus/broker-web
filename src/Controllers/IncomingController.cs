// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OregonNexus.Broker.Domain;
using System.Security.Claims;
using OregonNexus.Broker.SharedKernel;
using OregonNexus.Broker.Web.Models.Paginations;
using Ardalis.Specification;
using OregonNexus.Broker.Web.Specifications.Paginations;
using OregonNexus.Broker.Web.Models.IncomingRequests;
using OregonNexus.Broker.Web.ViewModels.IncomingRequests;
using Microsoft.EntityFrameworkCore;
using OregonNexus.Broker.Web.Services.PayloadContents;
using OregonNexus.Broker.Web.MapperExtensions.JsonDocuments;
using OregonNexus.Broker.Web.Models.JsonDocuments;
using System.Linq.Expressions;

namespace OregonNexus.Broker.Web.Controllers;

[Authorize(Policy = "TransferRecords")]
public class IncomingController : AuthenticatedController
{
    private readonly IRepository<EducationOrganization> _educationOrganizationRepository;
    private readonly IRepository<PayloadContent> _payloadContentRepository;
    private readonly IRepository<Request> _incomingRequestRepository;
    private readonly IPayloadContentService _payloadContentService;

    public IncomingController(
        IHttpContextAccessor httpContextAccessor,
        IRepository<EducationOrganization> educationOrganizationRepository,
        IRepository<PayloadContent> payloadContentRepository,
        IRepository<Request> incomingRequestRepository,
        IPayloadContentService payloadContentService) : base(httpContextAccessor)
    {
        _educationOrganizationRepository = educationOrganizationRepository;
        _payloadContentRepository = payloadContentRepository;
        _incomingRequestRepository = incomingRequestRepository;
        _payloadContentService = payloadContentService;
    }

    public async Task<IActionResult> Index(
        IncomingRequestModel model,
        CancellationToken cancellationToken)
    {
        RefreshSession();

        var searchExpressions = model.BuildSearchExpressions();

        var sortExpression = model.BuildSortExpression();

        var organizationId = GetFocusOrganizationId();
        Expression<Func<Request, bool>> focusOrganizationExpression = request =>
            request.ToEducationOrganizationId == organizationId;

        var specification = new SearchableWithPaginationSpecification<Request>.Builder(model.Page, model.Size)
            .WithAscending(model.IsAscending)
            .WithSortExpression(sortExpression)
            .WithSearchExpressions(searchExpressions)
            .WithSearchExpression(focusOrganizationExpression)
            .WithIncludeEntities(builder => builder
                .Include(incomingRequest => incomingRequest.EducationOrganization)
                .ThenInclude(educationOrganization => educationOrganization!.ParentOrganization)
                )
            .Build();

        var totalItems = await _incomingRequestRepository.CountAsync(
            specification,
            cancellationToken);

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
        var educationOrganizationList = await _educationOrganizationRepository.ListAsync();
        var educationOrganizations = educationOrganizationList
            .Where(educationOrganization => educationOrganization.Id != GetFocusOrganizationId()
                && educationOrganization.ParentOrganizationId is null
            )
            .ToList();

        var viewModel = new CreateIncomingRequestViewModel
        {
            EducationOrganizations = educationOrganizations
                
        };

        return View(viewModel);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateIncomingRequestViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var userId = Guid.Parse(User.FindFirstValue(claimType: ClaimTypes.NameIdentifier)!);

            var organizationId = GetFocusOrganizationId();

            Expression<Func<EducationOrganization, bool>> focusOrganizationExpression = request =>
                request.Id == organizationId;

            var specification = new SearchableWithPaginationSpecification<EducationOrganization>.Builder(1, 1)
            .WithSearchExpression(focusOrganizationExpression)
            .WithIncludeEntities(builder => builder
                .Include(educationOrganization => educationOrganization.ParentOrganization))
            .Build();

            var educationOrganization = (await _educationOrganizationRepository.ListAsync(specification)).SingleOrDefault();

            viewModel.ToDistrict = educationOrganization?.ParentOrganization?.Name;
            viewModel.ToSchool = educationOrganization?.Name;
            viewModel.ToEmail = User?.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

            var synergyStudentModel = viewModel.MapToSynergyJsonModel();
            var requestManifest = viewModel.MapToRequestManifestJsonModel();

            var today = DateTime.UtcNow;
            var incomingRequest = new Request
            {
                EducationOrganizationId = viewModel.EducationOrganizationId,
                ToEducationOrganizationId = GetFocusOrganizationId(),
                Student = synergyStudentModel, 
                RequestManifest = requestManifest,
                ResponseManifest = requestManifest,
                InitialRequestSentDate = today,
                RequestProcessUserId = userId,
                RequestStatus = viewModel.RequestStatus,
                CreatedAt = today,
                CreatedBy = userId
            };

            await _incomingRequestRepository.AddAsync(incomingRequest);
            await _incomingRequestRepository.SaveChangesAsync();

            await _payloadContentService.AddPayloadContentsAsync(viewModel.Files, viewModel.RequestId);

            return RedirectToAction(nameof(Index));
        }

        var educationOrganizationList = await _educationOrganizationRepository.ListAsync();
        var educationOrganizations = educationOrganizationList.Where(educationOrganization => educationOrganization.Id != GetFocusOrganizationId())
                .ToList();
        viewModel.EducationOrganizations = educationOrganizations;
        return View(viewModel);
    }

    public async Task<IActionResult> Update(Guid requestId)
    {
        var incomingRequest = await _incomingRequestRepository.GetByIdAsync(requestId);
        if (incomingRequest is null) return NotFound();

        var educationOrganizationList = await _educationOrganizationRepository.ListAsync();
        var educationOrganizations = educationOrganizationList.Where(educationOrganization => educationOrganization.Id != GetFocusOrganizationId())
                .ToList();

        var synergyStudentModel = incomingRequest.Student?.DeserializeFromJsonDocument<SynergyJsonModel>();

        var requestManifest = incomingRequest.RequestManifest?.DeserializeFromJsonDocument<RequestManifestJsonModel>();

        var viewModel = new CreateIncomingRequestViewModel
        {
            RequestId = incomingRequest.Id,
            EducationOrganizations = educationOrganizations,
            EducationOrganizationId = incomingRequest.EducationOrganizationId,
            SisNumber = synergyStudentModel?.Student?.SisNumber,
            Id = requestManifest?.Student?.Id,
            StudentUniqueId = requestManifest?.Student?.StudentUniqueId,
            FirstName = requestManifest?.Student?.FirstName,
            MiddleName = requestManifest?.Student?.MiddleName,
            LastSurname = requestManifest?.Student?.LastSurname,
            FromDistrict = requestManifest?.From?.District,
            FromSchool = requestManifest?.From?.School,
            FromEmail = requestManifest?.From?.Email,
            ToDistrict = requestManifest?.To?.District,
            ToSchool = requestManifest?.To?.School,
            ToEmail = requestManifest?.To?.Email,
            Note = requestManifest?.Note,
            Contents = requestManifest?.Contents,
            RequestStatus = incomingRequest.RequestStatus
        };

        return View(viewModel);
    }

    [HttpPut]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(CreateIncomingRequestViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var userId = Guid.Parse(User.FindFirstValue(claimType: ClaimTypes.NameIdentifier)!);

            var incomingRequest = await _incomingRequestRepository.GetByIdAsync(viewModel.RequestId);

            if (incomingRequest is null) return BadRequest();

            var synergyStudentModel = viewModel.MapToSynergyJsonModel();
            var requestManifest = viewModel.MapToRequestManifestJsonModel();

            var today = DateTime.UtcNow;

            incomingRequest.EducationOrganizationId = viewModel.EducationOrganizationId;
            incomingRequest.Student = synergyStudentModel;
            incomingRequest.RequestManifest = requestManifest;
            incomingRequest.ResponseManifest = requestManifest;
            incomingRequest.RequestStatus = viewModel.RequestStatus;
            incomingRequest.UpdatedAt = today;
            incomingRequest.UpdatedBy = userId;

            await _incomingRequestRepository.UpdateAsync(incomingRequest);
            await _incomingRequestRepository.SaveChangesAsync();

            await _payloadContentService.AddPayloadContentsAsync(viewModel.Files, viewModel.RequestId);

            return RedirectToAction(nameof(Index));
        }

        var educationOrganizationList = await _educationOrganizationRepository.ListAsync();
        var educationOrganizations = educationOrganizationList.Where(educationOrganization => educationOrganization.Id != GetFocusOrganizationId())
                .ToList();
        viewModel.EducationOrganizations = educationOrganizations;
        return View(viewModel);
    }
}
