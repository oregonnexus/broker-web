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
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Newtonsoft.Json;
using OregonNexus.Broker.Web.Models.JsonDocuments;

namespace OregonNexus.Broker.Web.Controllers;

[Authorize(Policy = "TransferRecords")]
public class IncomingController : Controller
{
    private readonly IRepository<EducationOrganization> _educationOrganizationRepository;
    private readonly IRepository<PayloadContent> _payloadContentRepository;
    private readonly IRepository<Request> _incomingRequestRepository;

    public IncomingController(
        IRepository<EducationOrganization> educationOrganizationRepository,
        IRepository<Request> incomingRequestRepository,
        IRepository<PayloadContent> payloadContentRepository)
    {
        _educationOrganizationRepository = educationOrganizationRepository;
        _incomingRequestRepository = incomingRequestRepository;
        _payloadContentRepository = payloadContentRepository;
    }

    public async Task<IActionResult> Index(
        IncomingRequestModel model,
        CancellationToken cancellationToken)
    {
        var searchExpressions = model.BuildSearchExpressions();

        var sortExpression = model.BuildSortExpression();

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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateIncomingRequestViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var userId = User.FindFirstValue(claimType: ClaimTypes.NameIdentifier)!;

            var synergyStudentModel = new SynergyJsonModel()
            {
                Student = new SynergyStudentJsonModel()
                {
                    SisNumber = viewModel.SisNumber
                }
            }.ToJsonDocument();

            var requestManifest = new RequestManifestJsonModel()
            {
                RequestId = Guid.NewGuid(),
                RequestType = "OregonNexus.Broker.Connector.Payload.StudentCumulativeRecord",
                Student = new StudentJsonModel()
                {
                    Id = viewModel.Id,
                    StudentUniqueId = viewModel.StudentUniqueId,
                    FirstName = viewModel.FirstName,
                    MiddleName = viewModel.MiddleName,
                    LastSurname = viewModel.LastSurname
                },
                From = new SchoolJsonModel()
                {
                    District = viewModel.FromDistrict,
                    School = viewModel.FromSchool,
                    Email = viewModel.FromEmail
                },
                To = new SchoolJsonModel()
                {
                    District = viewModel.ToDistrict,
                    School = viewModel.ToSchool,
                    Email = viewModel.ToEmail
                },
                Note = viewModel.Note,
                Contents = viewModel.Contents
            }.ToJsonDocument();

            var today = DateTime.UtcNow;
            var incomingRequest = new Request
            {
                EducationOrganizationId = viewModel.EducationOrganizationId,
                Student = synergyStudentModel, 
                RequestManifest = requestManifest,  
                InitialRequestSentDate = today,
                RequestProcessUserId = Guid.Parse(userId),
                RequestStatus = viewModel.RequestStatus,
                CreatedAt = today,
                CreatedBy = Guid.Parse(userId)
            };

            await _incomingRequestRepository.AddAsync(incomingRequest);
            await _incomingRequestRepository.SaveChangesAsync();

            var payloadContents = new List<PayloadContent>();

            foreach (var file in viewModel.Files)
            {
                var payloadContent = new PayloadContent
                {
                    RequestId = Guid.NewGuid(),
                    RequestResponse = RequestResponse.Response,
                    ContentType = "application/json",
                    BlobContent = Encoding.UTF8.GetBytes("YourBlobContentHere"),
                    XmlContent = XElement.Parse("<Student><Id>000000</Id><StudentUniqueId>0000000</StudentUniqueId><FirstName>John</FirstName><MiddleName>T</MiddleName><LastSurname>Doe</LastSurname></Student>")
                };

                payloadContents.Add(payloadContent);
            }

            await _payloadContentRepository.AddRangeAsync(payloadContents);
            await _payloadContentRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        var educationOrganizations = await _educationOrganizationRepository.ListAsync();
        viewModel.EducationOrganizations = educationOrganizations;
        return View(viewModel);
    }
}
