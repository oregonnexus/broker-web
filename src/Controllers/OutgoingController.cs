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
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using OregonNexus.Broker.Web.Models.JsonDocuments;

namespace OregonNexus.Broker.Web.Controllers;

[Authorize(Policy = "TransferRecords")]
public class OutgoingController : Controller
{
    private readonly IRepository<Request> _outgoingRequestRepository;
    private readonly IRepository<PayloadContent> _payloadContentRepository;
    private readonly IRepository<EducationOrganization> _educationOrganizationRepository;

    public OutgoingController(
        IRepository<Request> outgoingRequestRepository,
        IRepository<PayloadContent> payloadContentRepository,
        IRepository<EducationOrganization> educationOrganizationRepository)
    {
        _outgoingRequestRepository = outgoingRequestRepository;
        _payloadContentRepository = payloadContentRepository;
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

            var edfiStudentModel = new EdfiJsonModel()
            {
                Student = new EdfiStudentJsonModel()
                {
                    Id = viewModel.EdfiId,
                    StudentUniqueId = viewModel.EdfiStudentUniqueId
                }
            }.ToJsonDocument();

            var responseManifest = new ResponseManifestJsonModel()
            {
                RequestId = Guid.NewGuid(),
                ResponseType = "OregonNexus.Broker.Connector.Payload.StudentCumulativeRecord",
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
            var outgoingRequest = new Request
            {
                EducationOrganizationId = viewModel.EducationOrganizationId,
                Student = edfiStudentModel,
                ResponseManifest = responseManifest,
                InitialRequestSentDate = today,
                ResponseProcessUserId = Guid.Parse(userId),
                RequestStatus = viewModel.RequestStatus,
                CreatedAt = today,
                CreatedBy = Guid.Parse(userId)
            };

            await _outgoingRequestRepository.AddAsync(outgoingRequest);
            await _outgoingRequestRepository.SaveChangesAsync();

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
