﻿using Microsoft.AspNetCore.Mvc.Rendering;
using OregonNexus.Broker.Domain;
using System.ComponentModel.DataAnnotations;

namespace OregonNexus.Broker.Web.ViewModels.IncomingRequests;

public class CreateIncomingRequestViewModel
{
    public Guid RequestId { get; set; }

    public List<EducationOrganization> EducationOrganizations { get; set; } = new List<EducationOrganization>();

    [Display(Name = "Releasing School")]
    // [Required(ErrorMessage = "Education Organization is required")]
    public Guid? EducationOrganizationId { get; set; }

    [Display(Name = "Student")]
    // [Required(ErrorMessage = "Student is required")]
    public string? Student { get; set; } = string.Empty;

    [Display(Name = "SIS Number")]
    public string? SisNumber { get; set; }

    [Display(Name = "EdFi ID")]
    public string? Id { get; set; }

    [Display(Name = "Student ID")]
    public string? StudentUniqueId { get; set; }

    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [Display(Name = "Middle Name")]
    public string? MiddleName { get; set; }

    [Display(Name = "Last Name")]
    public string? LastSurname { get; set; }

    [Display(Name = "Birth Date")]
    public string? BirthDate { get; set; }

    [Display(Name = "Gender")]
    public string? Gender { get; set; }

    [Display(Name = "Grade")]
    public string? Grade { get; set; }

    [Display(Name = "Releasing District")]
    public string? FromDistrict { get; set; }

    [Display(Name = "Releasing School")]
    public string? FromSchool { get; set; }

    [Display(Name = "Releasing Clerk's Email")]
    public string? FromEmail { get; set; }

    [Display(Name = "Receiving District")]
    public string? ToDistrict { get; set; }

    [Display(Name = "Receiving School")]
    public string? ToSchool { get; set; }

    [Display(Name = "Receiving Clerk's Email")]
    public string? ToEmail { get; set; }

    [Display(Name = "Notes")]
    public string? Note { get; set; }

    [Display(Name = "Contents")]
    public List<string>? Contents { get; set; }

    [Display(Name = "Request Status")]
    // [Required(ErrorMessage = "Request Status is required")]
    public RequestStatus RequestStatus { get; set; }

    public List<RequestStatus> RequestStatuses => 
        RequestStatus != RequestStatus.Imported 
            ? new List<RequestStatus> { RequestStatus.Draft, RequestStatus.Sent } 
            : new List<RequestStatus> { RequestStatus.Received, RequestStatus.Declined };

    [Display(Name = "Files")]
    public IFormFileCollection Files { get; set; } = new FormFileCollection();

    public IEnumerable<SelectListItem> States { get; set; } = Enumerable.Empty<SelectListItem>();

    public IEnumerable<SelectListItem> Genders { get; set; } = Enumerable.Empty<SelectListItem>();
}
