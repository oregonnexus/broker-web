using System.ComponentModel;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Domain.Specifications;
using OregonNexus.Broker.Service;
using OregonNexus.Broker.Web.Constants.DesignSystems;

namespace OregonNexus.Broker.Web.Controllers;

[Authorize(Policy = "SuperAdmin")]
public partial class SettingsController : AuthenticatedController
{
    [HttpGet("/Settings/OutgoingPayload/{payload}")]
    public async Task<IActionResult> OutgoingPayload(string payload)
    {
        if (await FocusedToDistrict() is not null) return await FocusedToDistrict();

        var payloadAssembly = _connectorLoader.Payloads.Where(x => x.FullName == payload).First();

        var currentPayload = await _educationOrganizationPayloadSettings
            .FirstOrDefaultAsync(new PayloadSettingsByNameAndEdOrgIdSpec(payload, PayloadDirection.Outgoing, _focusedDistrictEdOrg!.Value));

        var contentTypes = _payloadContentTypeService.GetPayloadContentTypes() ?? Enumerable.Empty<PayloadContentTypeDisplay>();

        // Format for json on screen
        var settings = new List<dynamic>();
        if (currentPayload is not null && currentPayload?.Settings.Count() > 0)
        {
            foreach(var currentSettings in currentPayload.Settings)
            {
                settings.Add(new {
                    fullName = currentSettings.PayloadContentType,
                    displayName = contentTypes.Where(a => a.FullName == currentSettings.PayloadContentType).FirstOrDefault()!.DisplayName,
                    configuration = currentSettings.Settings
                });
            }
        }

        return View(new
        {
            ContentTypes = contentTypes,
            Payload = new
            {
                FullName = payload,
                ((DisplayNameAttribute)payloadAssembly
                    .GetCustomAttributes(false)
                    .First(x => x.GetType() == typeof(DisplayNameAttribute))).DisplayName,
                Settings = settings.ToJsonDocument() ?? "[]".ToJsonDocument()
            }
        });
    }

    [HttpPost("/Settings/OutgoingPayload/{payload}")]
    public async Task<IActionResult> UpdateOutgoingPayload(
        [FromRoute] string payload,
        [FromForm] string settings)
    {
        if (await FocusedToDistrict() is not null) return await FocusedToDistrict();

        // Transform incoming form json data to jsonnode
        var jsonSettings = JsonNode.Parse(settings)!.AsArray();

        // Start Payload Settings Content Types
        var payloadContentTypeSettings = new List<PayloadSettingsContentType>();
        
        foreach(var jsonSetting in jsonSettings)
        {
            payloadContentTypeSettings.Add(
                new PayloadSettingsContentType()
                {
                    PayloadContentType = jsonSetting["fullName"].ToString(),
                    Settings = jsonSetting["configuration"].ToString()
                }
            );
        }

        var currentPayload = await _educationOrganizationPayloadSettings
            .FirstOrDefaultAsync(new PayloadSettingsByNameAndEdOrgIdSpec(payload, PayloadDirection.Outgoing, _focusedDistrictEdOrg!.Value));

        if (currentPayload is not null)
        {
            currentPayload.Settings = payloadContentTypeSettings;
            await _educationOrganizationPayloadSettings.UpdateAsync(currentPayload);
        }
        else
        {
            await _educationOrganizationPayloadSettings.AddAsync(new EducationOrganizationPayloadSettings()
            {
                EducationOrganizationId = _focusedDistrictEdOrg!.Value,
                PayloadDirection = PayloadDirection.Outgoing,
                Payload = payload,
                Settings = payloadContentTypeSettings
            });
        }

        TempData[VoiceTone.Positive] = $"Updated Outgoing Payload.";

        return RedirectToAction("OutgoingPayload", new { payload });
    }
}