using System.Net;
using Microsoft.AspNetCore.Mvc;
using OregonNexus.Broker.Web.Controllers;
using src.Models.Students;
using src.Services.Students;

namespace src.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : AuthenticatedController
{
    private readonly IStudentService _studentService;
    public StudentsController(
        IHttpContextAccessor httpContextAccessor,
        IStudentService studentService) : base(httpContextAccessor)
    {
        _studentService = studentService;
    }


    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] StudentRequest request)
    {
        try
        {
            var students = await _studentService.GetAllAsync(request);
            if(!students.Any()) return NotFound();
            return Ok(students);
        }
        catch(Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Show(string id)
    {
        try
        {
            var student = await _studentService.GetById(id);
            return Ok(student);
        }
        catch(Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        }
    }
}
