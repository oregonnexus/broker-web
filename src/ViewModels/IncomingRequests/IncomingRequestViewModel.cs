using System.ComponentModel.DataAnnotations;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Web.Constants.DesignSystems;
using OregonNexus.Broker.Web.Extensions.RequestStatuses;

namespace OregonNexus.Broker.Web.ViewModels.IncomingRequests;

public class IncomingRequestViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Display(Name = "District")]
    public string District { get; set; } = string.Empty;

    [Display(Name = "School")]
    public string School { get; set; } = string.Empty;

    [Display(Name = "Student")]
    public string? Student { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Date")]
    public DateTime Date { get; set; }

    [Required]
    [Display(Name = "Status")]
    public string Status { get; set; } = string.Empty;

    // TODO: Map Status to proper Voice Tone.
    [Display(Name = "Status Tone")]
    public string StatusTone => VoiceTone.Positive;

    public IncomingRequestViewModel() { }

    public IncomingRequestViewModel(IncomingRequest incomingRequest)
    {
        Id = incomingRequest.Id;
        District = incomingRequest.EducationOrganization?.ParentOrganization?.Name ?? string.Empty;
        School = incomingRequest.EducationOrganization?.Name ?? string.Empty;
        Student = incomingRequest.Student;
        Date = incomingRequest.RequestDate;
        Status = incomingRequest.RequestStatus.ToFriendlyString();
    }

    public IncomingRequestViewModel(
        Guid id,
        string district,
        string school,
        string? student,
        DateTime date,
        string status)
    {
        Id = id;
        District = district;
        School = school;
        Student = student;
        Date = date;
        Status = status;
    }
}
