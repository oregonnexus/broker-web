// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OregonNexus.Broker.Web.Models;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using InertiaAdapter;

namespace OregonNexus.Broker.Web.Controllers;

[Authorize(Policy = "TransferRecords")]
public class OutgoingController : Controller
{
    private readonly IRepository<OutgoingRequest> _repo;
    
    public OutgoingController(IRepository<OutgoingRequest> repo)
    {
        _repo = repo;
    }
    
    public async Task<IActionResult> Index()
    {
        var data = await _repo.ListAsync();
        
        return View(data);
    }
}