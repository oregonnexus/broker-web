using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OregonNexus.Broker.SharedKernel;
using OregonNexus.Broker.Web.Controllers;
using src.Models.Students;
using OregonNexus.Broker.Service.Lookup;
using OregonNexus.Broker.Domain;

namespace OregonNexus.Broker.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/students")]
public class StudentsSearchController : AuthenticatedController<StudentsSearchController>
{
    private readonly StudentLookupService _studentLookupService;

    public StudentsSearchController(IHttpContextAccessor httpContextAccessor, StudentLookupService studentLookupService)
    {
        _studentLookupService = studentLookupService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return Ok();
    }

    [HttpGet]
    [Route("search")]
    public async Task<IActionResult> Search([FromQuery] string search)
    {
        try
        {
            var results = await _studentLookupService.SearchAsync(PayloadDirection.Incoming, search);

            return Ok(results);
        }
        catch(Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
    }

}
