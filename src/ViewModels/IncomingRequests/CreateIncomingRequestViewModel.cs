using OregonNexus.Broker.Domain;
using System.ComponentModel.DataAnnotations;

namespace OregonNexus.Broker.Web.ViewModels.IncomingRequests;

public class CreateIncomingRequestViewModel
{
    public List<EducationOrganization> EducationOrganizations { get; set; } = new List<EducationOrganization>();

    [Display(Name = "Education organization")]
    [Required(ErrorMessage = "Education Organization is required")]
    public Guid? EducationOrganizationId { get; set; }

    [Display(Name = "Student")]
    [Required(ErrorMessage = "Student is required")]
    public string? Student { get; set; } = string.Empty;

    public string? SisNumber { get; set; }

    public string? Id { get; set; }
    public string? StudentUniqueId { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastSurname { get; set; }

    public string? FromDistrict { get; set; }
    public string? FromSchool { get; set; }
    public string? FromEmail { get; set; }

    public string? ToDistrict { get; set; }
    public string? ToSchool { get; set; }
    public string? ToEmail { get; set; }

    public string? Note { get; set; }
    public List<string>? Contents { get; set; }

    [Display(Name = "Request Status")]
    [Required(ErrorMessage = "Request Status is required")]
    public RequestStatus RequestStatus { get; set; } = RequestStatus.Draft;
}
