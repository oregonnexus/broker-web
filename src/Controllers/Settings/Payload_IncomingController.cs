using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Domain.Specifications;
using OregonNexus.Broker.Web.Constants.DesignSystems;

namespace OregonNexus.Broker.Web.Controllers;

[Authorize(Policy = "SuperAdmin")]
public partial class SettingsController : AuthenticatedController
{
    [HttpGet("/Settings/IncomingPayload/{payload}")]
    public async Task<IActionResult> IncomingPayload(string payload)
    {
        if (await FocusedToDistrict() is not null) return await FocusedToDistrict();

        var payloadAssembly = _connectorLoader.Payloads.Where(x => x.FullName == payload).First();

        var currentPayload = await _educationOrganizationPayloadSettings
            .FirstOrDefaultAsync(new PayloadSettingsByNameAndEdOrgIdSpec(payload, PayloadDirection.Incoming, _focusedDistrictEdOrg!.Value));
        
        var currentDataConnector = currentPayload?.Settings.Where(i => i.PayloadContentType == "DataConnector").FirstOrDefault();

        var connectors = _connectorLoader.Connectors;

        // Create select menu
        var connectorListItems = new List<SelectListItem>();

        foreach(var connector in connectors)
        {
            connectorListItems.Add(new SelectListItem() {
                Text = ((DisplayNameAttribute)connector
                        .GetCustomAttributes(false)
                        .First(x => x.GetType() == typeof(DisplayNameAttribute))).DisplayName,
                Value = connector.FullName,
                Selected = (connector.FullName == currentDataConnector?.Settings) ? true : false
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
    public async Task<IActionResult> UpdateIncomingPayload([FromRoute] string payload)
    {
        if (await FocusedToDistrict() is not null) return await FocusedToDistrict();
        
        var currentPayload = await _educationOrganizationPayloadSettings
            .FirstOrDefaultAsync(new PayloadSettingsByNameAndEdOrgIdSpec(payload, PayloadDirection.Incoming, _focusedDistrictEdOrg!.Value));

        var payloadContentSettingsToSave = new List<PayloadSettingsContentType>();

        payloadContentSettingsToSave.Add(
            new PayloadSettingsContentType() {
                PayloadContentType = "DataConnector",
                Settings = Request.Form.Where(i => i.Key == "DataConnector").FirstOrDefault().Value.ToString()
            }
        );

        if (currentPayload is not null)
        {
            currentPayload.Settings = payloadContentSettingsToSave;
            await _educationOrganizationPayloadSettings.UpdateAsync(currentPayload);
        }
        else
        {
            await _educationOrganizationPayloadSettings.AddAsync(new EducationOrganizationPayloadSettings()
            {
                EducationOrganizationId = _focusedDistrictEdOrg!.Value,
                PayloadDirection = PayloadDirection.Incoming,
                Payload = payload,
                Settings = payloadContentSettingsToSave
            });
        }

        TempData[VoiceTone.Positive] = $"Updated Incoming Payload.";

        return RedirectToAction("IncomingPayload", new { payload });
    }
}