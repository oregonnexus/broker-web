using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OregonNexus.Broker.SharedKernel;
using OregonNexus.Broker.Web.Controllers;
using src.Models.Students;

namespace OregonNexus.Broker.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/students")]
public class StudentsSearchController : AuthenticatedController
{
    public StudentsSearchController(
        IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
    }

    [HttpGet]
    public IActionResult Index()
    {
        return Ok();
    }

    [HttpGet]
    [Route("search")]
    public IActionResult Search([FromQuery] string search)
    {
        try
        {
            var students = new List<dynamic>();
            
            students.Add(new
            {
                StudentId = "232323",
                FirstName = "John",
                LastName = "Doe",
                BirthDate = new DateOnly(2000, 1, 3),
                Gender = "M"
            });

            students.Add(new
            {
                StudentId = "234933",
                FirstName = "Jane",
                LastName = "Doe",
                BirthDate = new DateOnly(2003, 4, 5),
                Gender = "F"
            });

            var filteredStudents = students.Where(x => x.StudentId.ToLower().Contains(search) || x.FirstName.ToLower().Contains(search) || x.LastName.ToLower().Contains(search));

            return Ok(filteredStudents);
        }
        catch(Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
    }

}
