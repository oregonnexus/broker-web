using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Domain.Specifications;
using OregonNexus.Broker.Service.Lookup;
using OregonNexus.Broker.SharedKernel;
using OregonNexus.Broker.Web;

namespace OregonNexus.Broker.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/v1/directory")]
public class DirectoryController : Controller
{
    private readonly IReadRepository<EducationOrganization> _educationOrganizations;

    public DirectoryController(IReadRepository<EducationOrganization> educationOrganizations)
    {
        _educationOrganizations = educationOrganizations;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return Ok();
    }

    [HttpGet]
    [Route("search")]
    public async Task<IActionResult> Search([FromQuery] string domain)
    {
        try
        {
            var districtEdOrg = await _educationOrganizations.FirstOrDefaultAsync(new OrganizationByDomainSpec(domain));

            if (districtEdOrg is null)
            {
                throw new NoDistrictForDomainException();
            }
            
            var district = new District()
            {
                Id = districtEdOrg.Id,
                Name = districtEdOrg.Name,
                Number = districtEdOrg.Number,
                Address = (districtEdOrg.StreetNumberName is not null) ? new()
                {
                    StreetNumberName = districtEdOrg.StreetNumberName,
                    City = districtEdOrg.City,
                    StateAbbreviation = districtEdOrg.StateAbbreviation,
                    PostalCode = districtEdOrg.PostalCode
                } : null,
                Schools = new List<School>()
            };

            if (districtEdOrg.EducationOrganizations?.Count > 0)
            {
                foreach(var schoolEdOrg in districtEdOrg.EducationOrganizations)
                {
                    district.Schools.Add(new School()
                    {
                        Id = schoolEdOrg.Id,
                        Name = schoolEdOrg.Name,
                        Number = schoolEdOrg.Number,
                        Address = (schoolEdOrg.StreetNumberName is not null) ? new()
                        {
                            StreetNumberName = schoolEdOrg.StreetNumberName,
                            City = schoolEdOrg.City,
                            StateAbbreviation = schoolEdOrg.StateAbbreviation,
                            PostalCode = schoolEdOrg.PostalCode
                        } : null
                    });
                }
            }

            // var jsonResults = JsonSerializer.Serialize(results, new JsonSerializerOptions()
            // {
            //     ReferenceHandler = ReferenceHandler.IgnoreCycles,
            //     WriteIndented = true
            // });

            return Ok(district);
        }
        catch(NoDistrictForDomainException _)
        {
            var message = $"No district found for domain: {domain}";
            return NotFound(message.ToJsonDocument());
        }
        catch(Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, $"{ex.Message}\n\n{ex.StackTrace}");
        }
    }

}

public class NoDistrictForDomainException : Exception {}