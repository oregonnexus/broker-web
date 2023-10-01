// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OregonNexus.Broker.Web.Models;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using InertiaAdapter;
using OregonNexus.Broker.Web.Helpers;
using OregonNexus.Broker.Connector.Payload;

namespace OregonNexus.Broker.Web.Controllers;

[Authorize(Policy = "TransferRecords")]
public class OutgoingController : AuthenticatedController
{
    private readonly IRepository<Request> _repo;

    private readonly FocusHelper _focusHelper;
    
    public OutgoingController(IRepository<Request> repo, FocusHelper focusHelper)
    {
        _repo = repo;
        _focusHelper = focusHelper;
    }
    
    public async Task<IActionResult> Index()
    {
        var data = await _repo.ListAsync();
        
        return View(data);
    }

    // TO REMOVE
    public async Task<IActionResult> MakeRequest()
    {
        var details = new Dictionary<string, object>
        {
            {
                typeof(Student).FullName!,
                new Student
                {
                    LastName = "Doe",
                    FirstName = "John",
                    MiddleName = "T",
                    StudentNumber = "232434",
                    Grade = "03",
                    Birthdate = DateTime.Parse("08/06/2005")
                }
            }
        };
        
        // var request = new OutgoingRequest()
        // {
        //     RequestDate = DateTime.UtcNow,
        //     RequestStatus = RequestStatus.Sent,
        //     EducationOrganizationId = await _focusHelper.CurrentDistrictEdOrgFocus(),
        //     /*
        //     RequestDetails = new RequestDetails()
        //     {
        //         CreateDate = DateTime.UtcNow.AddHours(-1),
        //         PayloadType = typeof(StudentCumulativeRecord).FullName,
        //         Details = details
        //     }
        //     */
        // };
        // await _repo.AddAsync(request);
        
        return RedirectToAction("Index");
    }
}