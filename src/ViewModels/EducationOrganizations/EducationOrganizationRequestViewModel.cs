using System.ComponentModel.DataAnnotations;
using OregonNexus.Broker.Domain;

namespace OregonNexus.Broker.Web.ViewModels.EducationOrganizations;

public class EducationOrganizationRequestViewModel
{
    public Guid? Id { get; set; }

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

    public EducationOrganization ParentOrganization { get; set; } = new EducationOrganization();

    public EducationOrganizationRequestViewModel() { }

    public EducationOrganizationRequestViewModel(
        EducationOrganization educationOrganization)
    {
        Id = educationOrganization.Id;
        Name = educationOrganization.Name;
        Number = educationOrganization.Number ?? string.Empty;
        EducationOrganizationType = educationOrganization.EducationOrganizationType;
        ParentOrganizationId = educationOrganization.ParentOrganizationId;
        ParentOrganization = educationOrganization.ParentOrganization ?? new EducationOrganization();
    }

    public EducationOrganizationRequestViewModel(
        Guid? id,
        string name,
        string number,
        EducationOrganizationType educationOrganizationType,
        Guid? parentOrganizationId)
    {
        Id = id;
        Name = name;
        Number = number;
        EducationOrganizationType = educationOrganizationType;
        ParentOrganizationId = parentOrganizationId;
    }
}
