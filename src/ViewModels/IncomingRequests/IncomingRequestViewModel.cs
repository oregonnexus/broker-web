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

    [Display(Name = "District")]
    public string District { get; set; } = string.Empty;

    [Display(Name = "Receiving District")]
    public string ReceivingDistrict { get; set; } = string.Empty;

    [Display(Name = "School")]
    public string School { get; set; } = string.Empty;

    [Display(Name = "Student")]
    public string? Student { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Date")]
    public DateTimeOffset Date { get; set; }

    [Required]
    [Display(Name = "Status")]
    public string Status { get; set; } = string.Empty;

    // TODO: Map Status to proper Voice Tone.
    [Display(Name = "Status Tone")]
    public string StatusTone => VoiceTone.Positive;
    public IncomingRequestViewModel() { }

    public IncomingRequestViewModel(Request incomingRequest)
    {
        var requestManifest = incomingRequest.RequestManifest?.DeserializeFromJsonDocument<RequestManifestJsonModel>();
        var responseManifest = incomingRequest.ResponseManifest?.DeserializeFromJsonDocument<ResponseManifestJsonModel>();

        Id = incomingRequest.Id;
        District = requestManifest?.From?.District ?? string.Empty;
        ReceivingDistrict = requestManifest?.To?.District ?? string.Empty;
        School = requestManifest?.From?.School ?? string.Empty;
        Student = $"{requestManifest?.Student?.FirstName} {requestManifest?.Student?.LastSurname}";
        Date = incomingRequest.CreatedAt;
        Status = incomingRequest.RequestStatus == RequestStatus.Approved
                ? RequestStatus.Imported.ToFriendlyString() : incomingRequest.RequestStatus.ToFriendlyString();
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
