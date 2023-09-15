using System.ComponentModel.DataAnnotations;
using OregonNexus.Broker.Domain;

namespace OregonNexus.Broker.Web.Models;

public class IncomingRequestViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Display(Name = "District")]
    public required string District { get; set; }

    [Display(Name = "School")]
    public required string School { get; set; }

    [Display(Name = "Student")]
    public string? Student { get; set; }

    [Required]
    [Display(Name = "Date")]
    public DateTime Date { get; set; }

    [Required]
    [Display(Name = "Status")]
    public required string Status { get; set; }
}
