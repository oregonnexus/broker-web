using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.SharedKernel;
using static OregonNexus.Broker.Web.Constants.Claims.CustomClaimType;

namespace OregonNexus.Broker.Web.Controllers;

[Authorize(Policy = TransferOutgoingRecords)]
public class RequestsController : AuthenticatedController<RequestsController>
{
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<PayloadContent> _payloadContentRepository;

    public RequestsController(IRepository<Request> requestRepository,
        IRepository<PayloadContent> payloadContentRepository)
    {
        _requestRepository = requestRepository;
        _payloadContentRepository = payloadContentRepository;
    }

    public async Task<IActionResult> View(Guid id)
    {
        var request = await _requestRepository.GetByIdAsync(id);

        return View(request);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ViewAttachment(Guid id)
    {
        var payloadContent = await _payloadContentRepository.GetByIdAsync(id);

        Guard.Against.Null(payloadContent);
        Guard.Against.Null(payloadContent.ContentType, "ContentType", "ContentType missing from payload content.");

        if (payloadContent.JsonContent is not null)
        {
            return Ok(payloadContent.JsonContent.ToJsonString());
        }

        var stream = new MemoryStream();
        if (payloadContent.XmlContent is not null)
        {
            payloadContent.XmlContent.Save(stream);
        }
        if (payloadContent.BlobContent is not null)
        {
            await stream.WriteAsync(payloadContent.BlobContent);
            stream.Seek(0, SeekOrigin.Begin);
        }

        return new FileStreamResult(stream, payloadContent.ContentType);
    }
}
