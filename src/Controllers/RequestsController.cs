using System.Net;
using Microsoft.AspNetCore.Mvc;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.SharedKernel;
using OregonNexus.Broker.Web;
using OregonNexus.Broker.Web.Controllers;
using OregonNexus.Broker.Web.MapperExtensions.JsonDocuments;
using OregonNexus.Broker.Web.Models.JsonDocuments;
using src.Models.Students;
using src.Services.Students;

namespace src.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class RequestsController: AuthenticatedController
    {
            private readonly IRepository<Request> _requestRepository;
        public RequestsController(
            IHttpContextAccessor httpContextAccessor,
            IRepository<Request> requestRepository) : base(httpContextAccessor)
        {
            _requestRepository = requestRepository;
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromBody] RequestModel model)
        {
            try
            {
                var request = await _requestRepository.GetByIdAsync(id);
                if(request is null) return NotFound();
                var responseManifest = model.MapToResponseManifestJsonModel(request);
                request.ResponseManifest = responseManifest;
                await _requestRepository.UpdateAsync(request);
                return Ok(request);
            }
            catch(Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
