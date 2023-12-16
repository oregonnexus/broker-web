using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OregonNexus.Broker.Web.Controllers;

[Authorize(Policy = "SuperAdmin")]
public partial class SettingsController : AuthenticatedController
{
    [HttpGet("/Settings/IncomingPayload/{payload}")]
    public async Task<IActionResult> IncomingPayload(string payload)
    {
        if (await FocusedToDistrict() is not null) return await FocusedToDistrict();

        var payloadAssembly = _connectorLoader.Payloads.Where(x => x.FullName == payload).First();
        
        var connectors = _connectorLoader.Connectors;

        // Create select menu
        var connectorListItems = new List<SelectListItem>();
        foreach(var connector in connectors)
        {
            connectorListItems.Add(new SelectListItem() {
                Text = ((DisplayNameAttribute)connector
                        .GetCustomAttributes(false)
                        .First(x => x.GetType() == typeof(DisplayNameAttribute))).DisplayName,
                Value = connector.FullName
            });
        }
        connectorListItems = connectorListItems.OrderBy(x => x.Text).ToList();

        return View(
            new { Payload = 
                new { 
                    FullName = payload,
                    ((DisplayNameAttribute)payloadAssembly
                        .GetCustomAttributes(false)
                        .First(x => x.GetType() == typeof(DisplayNameAttribute))).DisplayName
                },
                ConnectorListItems = connectorListItems
            }
        );
    }

    [HttpPost("/Settings/IncomingPayload/{payload}")]
    public async Task<IActionResult> UpdateIncomingPayload(string payload)
    {
        return Ok("Test");
    }
}