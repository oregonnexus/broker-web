using System.Net;
using System.Text.Json;
using Ardalis.GuardClauses;
using Azure.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Domain.Specifications;
using OregonNexus.Broker.SharedKernel;
using OregonNexus.Broker.Web;
using OregonNexus.Broker.Web.Controllers;
using OregonNexus.Broker.Web.Utilities;

namespace OregonNexus.Broker.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/v1/requests")]
public class RequestsController : Controller
{
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<PayloadContent> _payloadContentRepository;
    private readonly IReadRepository<EducationOrganization> _edOrgRepo;

    public RequestsController(IRepository<Request> requestRepository, 
        IRepository<Message> messageRepository, 
        IRepository<PayloadContent> payloadContentRepository,
        IReadRepository<EducationOrganization> edOrgRepo)
    {
        _requestRepository = requestRepository;
        _messageRepository = messageRepository;
        _payloadContentRepository = payloadContentRepository;
        _edOrgRepo = edOrgRepo;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Receive([FromForm] string manifest, [FromForm] IList<IFormFile> files)
    {
        try
        {
            var mainfestJson = JsonSerializer.Deserialize<Manifest>(manifest, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true 
            });

            // Check if EdOrg exists
            var educationOrganizationId = mainfestJson?.To?.School?.Id;

            Guard.Against.Null(educationOrganizationId);

            var toEdOrg = await _edOrgRepo.GetByIdAsync(educationOrganizationId.Value);
            
            if (toEdOrg is null)
            {
                return NotFound($"EducationOrganization {educationOrganizationId} not found.");
            }

            // Create request
            var request = new Request()
            {
                EducationOrganizationId = educationOrganizationId.Value,
                RequestManifest = mainfestJson,
                RequestStatus = RequestStatus.Received,
                IncomingOutgoing = IncomingOutgoing.Outgoing
            };

            await _requestRepository.AddAsync(request);

            // Create message
            var message = new Message()
            {
                RequestId = request.Id,
                RequestResponse = RequestResponse.Response,
                MessageContents = JsonDocument.Parse(JsonSerializer.Serialize(request.RequestManifest))
            };
            await _messageRepository.AddAsync(message);

            // Add any attachments
            if (files is not null && files.Count > 0)
            {
                foreach(var file in files)
                {
                    var fileBlob = await FileHelpers
                        .ProcessFormFile<BufferedSingleFileUploadDb>(file, ModelState, new string[] { ".png", ".txt", ".pdf" }, 2097152);

                    var messageContent = new PayloadContent()
                    {
                        RequestId = request.Id,
                        MessageId = message.Id,
                        BlobContent = fileBlob,
                        ContentType = file.ContentType,
                        FileName = file.FileName
                    };

                    await _payloadContentRepository.AddAsync(messageContent);
                }
            }

            return Created("requests", request.Id.ToString());
        }
        catch(Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, $"{ex.Message}\n\n{ex.StackTrace}");
        }
    }

}