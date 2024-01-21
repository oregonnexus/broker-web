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

    [Display(Name = "Address")]
    public string? StreetNumberName { get; set; }

    [Display(Name = "City")]
    public string? City { get; set; }

    [Display(Name = "State")]
    public string? StateAbbreviation { get; set; }

    [Display(Name = "Zip Code")]
    public string? PostalCode { get; set; }

    public EducationOrganization ParentOrganization { get; set; } = new();

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
        StreetNumberName = educationOrganization.StreetNumberName;
        City = educationOrganization.City;
        StateAbbreviation = educationOrganization.StateAbbreviation;
        PostalCode = educationOrganization.PostalCode;
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
