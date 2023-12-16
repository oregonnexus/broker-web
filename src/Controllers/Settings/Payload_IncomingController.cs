using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OregonNexus.Broker.Web.Controllers;

[Authorize(Policy = "SuperAdmin")]
public partial class SettingsController : AuthenticatedController
{
    [HttpGet("/Settings/IncomingPayload/{payload}")]
    public async Task<IActionResult> IncomingPayload(string payload)
    {
        if (await FocusedToDistrict() is not null) return await FocusedToDistrict();
        /*
        var connectorDictionary = _connectorLoader.Assemblies.Where(x => x.Key == assembly).FirstOrDefault();
        ArgumentException.ThrowIfNullOrEmpty(assembly);
        var connector = connectorDictionary.Value;

        // Get configurations for connector - TO FIX!
        var configurations = _connectorLoader.GetConfigurations(connector);

        var forms = new List<dynamic>();

        foreach(var configType in configurations)
        {
            var configModel = await _configurationSerializer.DeseralizeAsync(configType, _focusedDistrictEdOrg.Value);
            var displayName = (DisplayNameAttribute)configType.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DisplayNameAttribute)).FirstOrDefault()!;

            forms.Add(
                new { 
                    displayName = displayName.DisplayName, 
                    html = ModelFormBuilderHelper.HtmlForModel(configModel) 
                }
            );
        }

        return View(forms);
        */
        return View(new { Payload = payload });
    }
}