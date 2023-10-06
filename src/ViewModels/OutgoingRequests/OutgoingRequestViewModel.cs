using System.ComponentModel.DataAnnotations;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Web.Constants.DesignSystems;
using OregonNexus.Broker.Web.Extensions.RequestStatuses;
using OregonNexus.Broker.Web.Models.JsonDocuments;

namespace OregonNexus.Broker.Web.ViewModels.OutgoingRequests;

public class OutgoingRequestViewModel
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
    public DateTimeOffset Date { get; set; }

    [Required]
    [Display(Name = "Status")]
    public string Status { get; set; } = string.Empty;

    // TODO: Map Status to proper Voice Tone.
    [Display(Name = "Status Tone")]
    public string StatusTone => VoiceTone.Positive;

    public OutgoingRequestViewModel() { }

    public OutgoingRequestViewModel(Request outgoingRequest)
    {
        var responseManifest = outgoingRequest.ResponseManifest?.DeserializeFromJsonDocument<ResponseManifestJsonModel>();

        Id = outgoingRequest.Id;
        District = responseManifest?.To?.District ?? string.Empty;
        School = responseManifest?.To?.School ?? string.Empty;
        Student = $"{responseManifest?.Student?.FirstName} {responseManifest?.Student?.LastSurname}";
        Date = outgoingRequest.CreatedAt;
        Status = outgoingRequest.RequestStatus == RequestStatus.Sent
                ? RequestStatus.WaitingApproval.ToFriendlyString() : outgoingRequest.RequestStatus.ToFriendlyString();
    }

    public OutgoingRequestViewModel(
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
