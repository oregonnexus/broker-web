using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Domain.Specifications;
using OregonNexus.Broker.SharedKernel;
using OregonNexus.Broker.Web.Constants.DesignSystems;
using OregonNexus.Broker.Web.Extensions.Genders;
using OregonNexus.Broker.Web.Extensions.States;
using OregonNexus.Broker.Web.ViewModels.Mappings;
using OregonNexus.Broker.Web.ViewModels.Requests;
using static OregonNexus.Broker.Web.Constants.Claims.CustomClaimType;

namespace OregonNexus.Broker.Web.Controllers;

[Authorize(Policy = TransferIncomingRecords)]
public class MappingController : AuthenticatedController<MappingController>
{
    private readonly IReadRepository<EducationOrganization> _educationOrganizationRepository;
    private readonly IRepository<Request> _incomingRequestRepository;
    private readonly IRepository<Mapping> _mappingRepository;

    public MappingController(
        IReadRepository<EducationOrganization> educationOrganizationRepository,
        IRepository<Request> incomingRequestRepository,
        IRepository<Mapping> mappingRepository)
    {
        _educationOrganizationRepository = educationOrganizationRepository;
        _incomingRequestRepository = incomingRequestRepository;
        _mappingRepository = mappingRepository;
    }

    public async Task<IActionResult> Index(Guid id)
    {
        var incomingRequest = await _incomingRequestRepository.GetByIdAsync(id);
        if (incomingRequest is null) return NotFound();

        var mappings = await _mappingRepository.ListAsync(new MappingByRequestId(incomingRequest.Id));

        if (mappings is not null && mappings.Count > 0)
        {
            return RedirectToAction(nameof(Edit), new { id = mappings.FirstOrDefault()!.Id } );
        }

        return View(incomingRequest);
    }

    [HttpPost]
    public async Task<IActionResult> Prepare(Guid id)
    {
        var incomingRequest = await _incomingRequestRepository.FirstOrDefaultAsync(new RequestByIdwithEdOrgs(id));

        Guard.Against.Null(incomingRequest);

        incomingRequest.RequestStatus = RequestStatus.WaitingToPrepare;

        await _incomingRequestRepository.UpdateAsync(incomingRequest);

        TempData[VoiceTone.Positive] = $"Preparing mapping for request ({incomingRequest.Id}).";
        return RedirectToAction(nameof(Index), new { id = id });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var mapping = await _mappingRepository.GetByIdAsync(id);
        if (mapping is null) return NotFound();

        var incomingRequest = await _incomingRequestRepository.GetByIdAsync(mapping.RequestId!.Value);
        if (incomingRequest is null) return NotFound();

        var mappings = await _mappingRepository.ListAsync(new MappingByRequestId(mapping.RequestId!.Value));

        var properties = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => p.FullName == mapping.MappingType).FirstOrDefault()!.GetProperties();

        var viewModel = new MappingViewModel()
        {
            MappingId = mapping.Id,
            RequestMappings = mappings,
            Mapping = mapping,
            Properties = properties
        };

        return View(viewModel);
    }

    [HttpPut]
    public async Task<IActionResult> Update(Guid id)
    {
        return RedirectToAction("Edit", new { id = id });
    }
}