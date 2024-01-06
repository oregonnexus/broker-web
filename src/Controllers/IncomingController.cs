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
using static OregonNexus.Broker.Web.Constants.Claims.CustomClaimType;
using OregonNexus.Broker.Web.Extensions.States;
using OregonNexus.Broker.Web.Extensions.Genders;
using src.Models.ProgramAssociations;
using src.Models.Courses;
using OregonNexus.Broker.Connector.Payload;
using OregonNexus.Broker.Web.Helpers;

namespace OregonNexus.Broker.Web.Controllers;

[Authorize(Policy = TransferIncomingRecords)]
public class IncomingController : AuthenticatedController<IncomingController>
{
    private readonly IRepository<EducationOrganization> _educationOrganizationRepository;
    private readonly IRepository<PayloadContent> _payloadContentRepository;
    private readonly IRepository<Request> _incomingRequestRepository;
    private readonly IPayloadContentService _payloadContentService;
    private readonly FocusHelper _focusHelper;

    public IncomingController(
        IHttpContextAccessor httpContextAccessor,
        IRepository<EducationOrganization> educationOrganizationRepository,
        IRepository<PayloadContent> payloadContentRepository,
        IRepository<Request> incomingRequestRepository,
        IPayloadContentService payloadContentService,
        FocusHelper focusHelper)
    {
        _educationOrganizationRepository = educationOrganizationRepository;
        _payloadContentRepository = payloadContentRepository;
        _incomingRequestRepository = incomingRequestRepository;
        _payloadContentService = payloadContentService;
        _focusHelper = focusHelper;
    }

    public async Task<IActionResult> Index(
        IncomingRequestModel model,
        CancellationToken cancellationToken)
    {
        RefreshSession();
        var searchExpressions = new List<Expression<Func<Request, bool>>>(); 
        searchExpressions = model.BuildSearchExpressions();

        var schools = await _focusHelper.GetFocusedSchools();

        searchExpressions.Add(x => schools.Contains(x.EducationOrganization));

        var sortExpression = model.BuildSortExpression();

        var organizationName = GetFocusOrganizationDistrict();

        var specification = new SearchableWithPaginationSpecification<Request>.Builder(model.Page, model.Size)
            .WithAscending(model.IsAscending)
            .WithSortExpression(sortExpression)
            .WithSearchExpressions(searchExpressions)
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
        
        if (!string.IsNullOrWhiteSpace(model.SearchBy))
        {
            incomingRequestViewModels = incomingRequestViewModels
                .Where(request => request.Student?.ToLower().Contains(model.SearchBy) is true || request.District.ToLower().Contains(model.SearchBy)
                 || request.School.ToLower().Contains(model.SearchBy));
            totalItems = incomingRequestViewModels.Count();
        }

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
            .Where(educationOrganization => educationOrganization.ParentOrganizationId is not null)
            .Where(educationOrganization => educationOrganization.Id != GetFocusOrganizationId())
            .ToList();

        var viewModel = new CreateIncomingRequestViewModel
        {
            EducationOrganizations = educationOrganizations,
            States = States.GetSelectList(),
            Genders = Genders.GetSelectList()
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

            var student = new Student() {
                LastName = viewModel.LastSurname,
                FirstName = viewModel.FirstName,
                MiddleName = viewModel.MiddleName,
                StudentNumber = viewModel.StudentUniqueId,
                Grade = viewModel.Grade,
                Gender = viewModel.Gender,
                Birthdate = DateOnly.Parse(viewModel.BirthDate!)
            };

            var incomingRequest = new Request
            {
                EducationOrganizationId = GetFocusOrganizationId(),
                Student = new StudentRequest() {
                    Student = student
                }, 
                RequestManifest = new Manifest() {
                    RequestType = typeof(StudentCumulativeRecord).FullName!,
                    Student = student
                },
                ResponseManifest = null,
                RequestProcessUserId = userId,
                RequestStatus = RequestStatus.Draft
            };

            await _incomingRequestRepository.AddAsync(incomingRequest);
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
        var educationOrganizations = educationOrganizationList
            .Where(educationOrganization => educationOrganization.ParentOrganizationId is not null)
            .Where(educationOrganization => educationOrganization.Id != GetFocusOrganizationId())
            .ToList();

        var requestManifest = incomingRequest.RequestManifest;

        var viewModel = new CreateIncomingRequestViewModel
        {
            RequestId = incomingRequest.Id,
            EducationOrganizations = educationOrganizations,
            EducationOrganizationId = incomingRequest.EducationOrganizationId,
            StudentUniqueId = requestManifest?.Student?.StudentNumber,
            FirstName = requestManifest?.Student?.FirstName,
            MiddleName = requestManifest?.Student?.MiddleName,
            LastSurname = requestManifest?.Student?.LastName,
            BirthDate = requestManifest?.Student?.Birthdate.Value.ToString("yyyy-MM-dd"),
            Gender = requestManifest?.Student?.Gender,
            Grade = requestManifest?.Student?.Grade,
            FromDistrict = requestManifest?.From?.District,
            FromSchool = requestManifest?.From?.School,
            FromEmail = requestManifest?.From?.Email,
            ToDistrict = requestManifest?.To?.District,
            ToSchool = requestManifest?.To?.School,
            ToEmail = requestManifest?.To?.Email,
            Note = requestManifest?.Note,
            Contents = requestManifest?.Contents?.Select(x => x.FileName).ToList(),
            RequestStatus = incomingRequest.RequestStatus,
            States = States.GetSelectList(),
            Genders = Genders.GetSelectList()
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
            viewModel.ToDistrict = GetFocusOrganizationDistrict();
            viewModel.ToSchool = GetFocusOrganizationSchool();
            viewModel.ToEmail = User?.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var incomingRequest = await _incomingRequestRepository.GetByIdAsync(viewModel.RequestId);

            if (incomingRequest is null) return BadRequest();

            incomingRequest.EducationOrganizationId = viewModel.EducationOrganizationId;
            incomingRequest.Student!.Student!.LastName = viewModel.LastSurname;
            incomingRequest.Student!.Student!.FirstName = viewModel.FirstName;
            incomingRequest.Student!.Student!.MiddleName = viewModel.MiddleName;
            incomingRequest.Student!.Student!.Gender = viewModel.Gender;
            incomingRequest.Student!.Student!.Grade = viewModel.Grade;
            incomingRequest.Student!.Student!.StudentNumber = viewModel.StudentUniqueId;

            incomingRequest.RequestManifest.Student.StudentNumber = viewModel.StudentUniqueId;
            incomingRequest.ResponseManifest = null;
            incomingRequest.RequestStatus = viewModel.RequestStatus;

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

    public async Task<IActionResult> Mapping(Guid requestId)
    {
        var incomingRequest = await _incomingRequestRepository.GetByIdAsync(requestId);
        if (incomingRequest is null) return NotFound();

        var educationOrganizationList = await _educationOrganizationRepository.ListAsync();
        var educationOrganizations = educationOrganizationList
            .Where(educationOrganization => educationOrganization.ParentOrganizationId is not null)
            .Where(educationOrganization => educationOrganization.Id != GetFocusOrganizationId())
            .ToList();

        var viewModel = new CreateIncomingRequestViewModel
        {
            RequestId = incomingRequest.Id,
            EducationOrganizations = educationOrganizations,
            EducationOrganizationId = incomingRequest.EducationOrganizationId,
            RequestStatus = incomingRequest.RequestStatus,
            States = States.GetSelectList(),
            Genders = Genders.GetSelectList()
        };

        return View(viewModel);
    }
}
