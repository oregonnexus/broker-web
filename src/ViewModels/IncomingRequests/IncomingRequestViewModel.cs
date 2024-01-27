using System.ComponentModel.DataAnnotations;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Web.Constants.DesignSystems;
using OregonNexus.Broker.Web.Extensions.RequestStatuses;
using OregonNexus.Broker.Web.Models.JsonDocuments;
using src.Models.Courses;
using src.Models.ProgramAssociations;

namespace OregonNexus.Broker.Web.ViewModels.IncomingRequests;

public class IncomingRequestViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Display(Name = "Releasing District")]
    public string ReleasingDistrict { get; set; } = string.Empty;

    [Display(Name = "Releasing School")]
    public string ReleasingSchool { get; set; } = string.Empty;

    [Display(Name = "Receiving District")]
    public string ReceivingDistrict { get; set; } = string.Empty;

    [Display(Name = "Receiving School")]
    public string ReceivingSchool { get; set; } = string.Empty;

    [Display(Name = "Student")]
    public string? Student { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Date")]
    public DateTimeOffset? Date { get; set; }

    [Required]
    [Display(Name = "Status")]
    public string Status { get; set; } = string.Empty;

    // TODO: Map Status to proper Voice Tone.
    [Display(Name = "Status Tone")]
    public string StatusTone => VoiceTone.Positive;
    public IncomingRequestViewModel() { }

    public IncomingRequestViewModel(Request incomingRequest)
    {

        Id = incomingRequest.Id;
        ReleasingDistrict = incomingRequest.RequestManifest?.To?.District?.Name ?? string.Empty;
        ReleasingSchool = incomingRequest.RequestManifest?.To?.School?.Name ?? string.Empty;
        ReceivingDistrict = incomingRequest.EducationOrganization?.ParentOrganization?.Name ?? string.Empty;
        ReceivingSchool = incomingRequest.EducationOrganization?.Name ?? string.Empty;
        Student = $"{incomingRequest.RequestManifest?.Student?.FirstName} {incomingRequest.RequestManifest?.Student?.LastName}";
        Date = incomingRequest.InitialRequestSentDate;
        Status = incomingRequest.RequestStatus == RequestStatus.Approved
                ? RequestStatus.Imported.ToFriendlyString() : incomingRequest.RequestStatus.ToFriendlyString();
    }
    public IncomingRequestViewModel(
        Guid id,
        string releasingDistrict,
        string releasingSchool,
        string receivingDistrict,
        string receivingSchool,
        string? student,
        DateTime date,
        string status)
    {
        Id = id;
        ReleasingDistrict = releasingDistrict;
        ReleasingSchool = releasingSchool;
        ReceivingDistrict = receivingDistrict;
        ReceivingSchool = receivingSchool;
        Student = student;
        Date = date;
        Status = status;
    }
}
