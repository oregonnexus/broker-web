using OregonNexus.Broker.Domain;
using System.ComponentModel.DataAnnotations;

namespace OregonNexus.Broker.Web.ViewModels.OutgoingRequests;

public class CreateOutgoingRequestViewModel
{
    public List<EducationOrganization> EducationOrganizations { get; set; } = new List<EducationOrganization>();

    [Display(Name = "Education organization")]
    [Required(ErrorMessage = "Education Organization is required")]
    public Guid? EducationOrganizationId { get; set; }

    [Display(Name = "Student")]
    [Required(ErrorMessage = "Student is required")]
    public string? Student { get; set; } = string.Empty;

    [Display(Name = "Request Status")]
    [Required(ErrorMessage = "Request Status is required")]
    public RequestStatus RequestStatus { get; set; } = RequestStatus.Draft;

}

