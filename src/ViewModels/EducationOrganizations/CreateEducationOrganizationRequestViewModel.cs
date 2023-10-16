using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using OregonNexus.Broker.Domain;

namespace OregonNexus.Broker.Web.ViewModels.EducationOrganizations;

public class CreateEducationOrganizationRequestViewModel
{
    [Display(Name = "Education Organization ID")]
    public Guid? EducationOrganizationId { get; set; }

    [Required]
    [Display(Name = "Name")]
    public string Name { get; set; } = default!;

    [Required]
    [Display(Name = "Number")]
    public string Number { get; set; } = default!;

    [Required]
    [Display(Name = "Type")]
    public EducationOrganizationType EducationOrganizationType { get; set; } = default!;

    [Display(Name = "District")]
    public Guid? ParentOrganizationId { get; set; }

    [Display(Name = "Address")]
    public Address Address {get;set;} = new();

    [Display(Name = "Education Organizations")]
    public IEnumerable<SelectListItem> EducationOrganizations { get; set; } = Enumerable.Empty<SelectListItem>();

    [Display(Name = "States")]
    public IEnumerable<SelectListItem> States { get; set; } = Enumerable.Empty<SelectListItem>();
}
