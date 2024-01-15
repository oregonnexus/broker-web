using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.SharedKernel;
using OregonNexus.Broker.Web.Extensions.Genders;
using OregonNexus.Broker.Web.Extensions.States;
using OregonNexus.Broker.Web.ViewModels.IncomingRequests;
using static OregonNexus.Broker.Web.Constants.Claims.CustomClaimType;

namespace OregonNexus.Broker.Web.Controllers;

[Authorize(Policy = TransferIncomingRecords)]
public class MappingController : AuthenticatedController<IncomingController>
{
    private readonly IReadRepository<EducationOrganization> _educationOrganizationRepository;
    private readonly IRepository<Request> _incomingRequestRepository;
    
    public MappingController(
        IReadRepository<EducationOrganization> educationOrganizationRepository,
        IRepository<Request> incomingRequestRepository)
    {
        _educationOrganizationRepository = educationOrganizationRepository;
        _incomingRequestRepository = incomingRequestRepository;
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