using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OregonNexus.Broker.SharedKernel;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Web.Helpers;
using OregonNexus.Broker.Web.Constants.DesignSystems;
using OregonNexus.Broker.Web.Models.OutgoingRequests;
using OregonNexus.Broker.Web.Models.Paginations;
using OregonNexus.Broker.Web.Specifications.Paginations;
using Ardalis.Specification;
using OregonNexus.Broker.Web.ViewModels.EducationOrganizations;
using System.Linq.Expressions;
using OregonNexus.Broker.Web.Extensions.States;

namespace OregonNexus.Broker.Web.Controllers;

[Authorize(Policy = "SuperAdmin")]
public class EducationOrganizationsController : AuthenticatedController
{
    private readonly IRepository<EducationOrganization> _educationOrganizationRepository;
    private readonly EducationOrganizationHelper _educationOrganizationHelper;
    public EducationOrganizationsController(
        IHttpContextAccessor httpContextAccessor,
        IRepository<EducationOrganization> educationOrganizationRepository,
        EducationOrganizationHelper educationOrganizationHelper) : base(httpContextAccessor)
    {
        _educationOrganizationRepository = educationOrganizationRepository;
        _educationOrganizationHelper = educationOrganizationHelper;
    }

    public async Task<IActionResult> Index(
      EducationOrganizationRequestModel model,
      CancellationToken cancellationToken)
    {
        RefreshSession();

        var searchExpressions = model.BuildSearchExpressions();

        var sortExpression = model.BuildSortExpression();

        var specification = new SearchableWithPaginationSpecification<EducationOrganization>.Builder(model.Page, model.Size)
            .WithAscending(model.IsAscending)
            .WithSortExpression(sortExpression)
            .WithSearchExpressions(searchExpressions)
            .WithIncludeEntities(builder => builder
                .Include(educationOrganization => educationOrganization.ParentOrganization))
            .Build();

        var totalItems = await _educationOrganizationRepository.CountAsync(
            specification,
            cancellationToken);

        var educationOrganizations = await _educationOrganizationRepository.ListAsync(
            specification,
            cancellationToken);

        var educationOrganizationViewModels = educationOrganizations
            .Select(educationOrganization => new EducationOrganizationRequestViewModel(educationOrganization));

        var result = new PaginatedViewModel<EducationOrganizationRequestViewModel>(
            educationOrganizationViewModels,
            totalItems,
            model.Page,
            model.Size,
            model.SortBy,
            model.SortDir,
            model.SearchBy);

        return View(result);
    }
    
    [HttpGet("search")]
    public async Task<IActionResult> Search(
      EducationOrganizationRequestModel model,
      CancellationToken cancellationToken)
    {
        RefreshSession();

        var searchExpressions = model.BuildSearchExpressions();

        var sortExpression = model.BuildSortExpression();

        var specification = new SearchableWithPaginationSpecification<EducationOrganization>.Builder(model.Page, model.Size)
            .WithAscending(model.IsAscending)
            .WithSortExpression(sortExpression)
            .WithSearchExpressions(searchExpressions)
            .WithIncludeEntities(builder => builder
                .Include(educationOrganization => educationOrganization.ParentOrganization)
            )
            .Build();

        var totalItems = await _educationOrganizationRepository.CountAsync(
            specification,
            cancellationToken);

        var educationOrganizations = await _educationOrganizationRepository.ListAsync(
            specification,
            cancellationToken);

        var educationOrganizationViewModels = educationOrganizations
            .Select(educationOrganization => new EducationOrganizationRequestViewModel(educationOrganization));

        var result = new PaginatedViewModel<EducationOrganizationRequestViewModel>(
            educationOrganizationViewModels,
            totalItems,
            model.Page,
            model.Size,
            model.SortBy,
            model.SortDir,
            model.SearchBy);

        return Ok(result);
    }
    public async Task<IActionResult> Create()
    {
        var educationOrganizations = await _educationOrganizationHelper.GetDistrictsOrganizationsSelectList();
        var viewModel = new CreateEducationOrganizationRequestViewModel
        {
            EducationOrganizations = educationOrganizations,
            States = States.GetSelectList()
        };

        return View(viewModel);
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> Create(CreateEducationOrganizationRequestViewModel data)
    {
        if (!ModelState.IsValid) { TempData[VoiceTone.Critical] = "Organization not created."; return RedirectToAction(nameof(Create)); }
        
        var organization = new EducationOrganization()
        {
            Id = Guid.NewGuid(),
            ParentOrganizationId = data.EducationOrganizationType == EducationOrganizationType.School ? data.ParentOrganizationId : null,
            Name = data.Name,
            Number = data.Number,
            EducationOrganizationType = data.EducationOrganizationType,
            StreetNumberName = data.StreetNumberName
        };

        await _educationOrganizationRepository.AddAsync(organization);

        TempData[VoiceTone.Positive] = $"Created organization {organization.Name} ({organization.Id}).";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Update(Guid Id)
    {
       Expression<Func<EducationOrganization, bool>> focusOrganizationExpression = request =>
            request.Id == Id;

        var specification = new SearchableWithPaginationSpecification<EducationOrganization>.Builder(1, -1)
            .WithSearchExpression(focusOrganizationExpression)
            .Build();

        var organizations = await _educationOrganizationRepository.ListAsync(specification,CancellationToken.None);

        var organization = organizations.FirstOrDefault();
        var organizationViewModel = new CreateEducationOrganizationRequestViewModel();

        if (organization is not null)
        {
            organizationViewModel = new CreateEducationOrganizationRequestViewModel()
            {
                EducationOrganizationId = organization.Id,
                ParentOrganizationId = organization.ParentOrganizationId,
                Name = organization.Name!,
                EducationOrganizationType = organization.EducationOrganizationType,
                Number = organization.Number!,
                StreetNumberName = organization.StreetNumberName!,
                States = States.GetSelectList()
            };
        }

        organizationViewModel.EducationOrganizations = await _educationOrganizationHelper.GetDistrictsOrganizationsSelectList(Id);

        return View(organizationViewModel);
    }

    [ValidateAntiForgeryToken]
    [HttpPatch]
    public async Task<IActionResult> Update(CreateEducationOrganizationRequestViewModel data)
    {
        if (!data.EducationOrganizationId.HasValue) { throw new ArgumentNullException("EducationOrganizationId required."); }
        
        // Get existing organization
       Expression<Func<EducationOrganization, bool>> focusOrganizationExpression = request =>
            request.Id == data.EducationOrganizationId;

        var specification = new SearchableWithPaginationSpecification<EducationOrganization>.Builder(1, -1)
            .WithSearchExpression(focusOrganizationExpression)
            .Build();

        var organizations = await _educationOrganizationRepository.ListAsync(specification,CancellationToken.None);

        var organization = organizations.FirstOrDefault();

        if (organization is null) { throw new ArgumentException("Not a valid organization."); }

        if (!ModelState.IsValid) { TempData[VoiceTone.Critical] = "Organization not updated."; return View("Edit"); }

        // Prepare organization object
        organization.Name = data.Name;
        if (data.EducationOrganizationType == EducationOrganizationType.School)
        {
            organization.ParentOrganizationId = data.ParentOrganizationId;
        }
        else
        {
            organization.ParentOrganizationId = null;
        }
        organization.Number = data.Number;
        organization.EducationOrganizationType = data.EducationOrganizationType;
        organization.StreetNumberName = data.StreetNumberName;

        await _educationOrganizationRepository.UpdateAsync(organization);

        TempData[VoiceTone.Positive] = $"Updated organization {organization.Name} ({organization.Id}).";

        return RedirectToAction(nameof(Index));
    }

    [ValidateAntiForgeryToken]
    [HttpDelete]
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id is null) { throw new ArgumentNullException("Missing id for organization to delete."); }
        
        var organization = await _educationOrganizationRepository.GetByIdAsync(id.Value);

        if (organization is null) { throw new ArgumentException("Not a valid organization."); }

        await _educationOrganizationRepository.DeleteAsync(organization);

        TempData[VoiceTone.Positive] = $"Deleted organization {organization.Name} ({organization.Id}).";

        return RedirectToAction(nameof(Index));
    }
}
