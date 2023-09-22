﻿using OregonNexus.Broker.Domain;
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

    [Display(Name = "SIS Number")]
    public string? SisNumber { get; set; }

    [Display(Name = "EdFi ID")]
    public string? Id { get; set; }

    [Display(Name = "Student Unique ID")]
    public string? StudentUniqueId { get; set; }

    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [Display(Name = "Middle Name")]
    public string? MiddleName { get; set; }

    [Display(Name = "Last Name")]
    public string? LastSurname { get; set; }

    [Display(Name = "From District")]
    public string? FromDistrict { get; set; }

    [Display(Name = "From School")]
    public string? FromSchool { get; set; }

    [Display(Name = "From Email")]
    public string? FromEmail { get; set; }

    [Display(Name = "To District")]
    public string? ToDistrict { get; set; }

    [Display(Name = "To School")]
    public string? ToSchool { get; set; }

    [Display(Name = "To Email")]
    public string? ToEmail { get; set; }

    [Display(Name = "Note")]
    public string? Note { get; set; }

    [Display(Name = "Contents")]
    public List<string>? Contents { get; set; }

    [Display(Name = "Request Status")]
    [Required(ErrorMessage = "Request Status is required")]
    public RequestStatus RequestStatus { get; set; } = RequestStatus.Draft;

    [Display(Name = "Files")]
    public IFormFileCollection Files { get; set; } = new FormFileCollection();
}
