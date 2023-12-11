using System.ComponentModel;
using System.Dynamic;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using OregonNexus.Broker.Connector;
using OregonNexus.Broker.Connector.Configuration;
using OregonNexus.Broker.Connector.Payload;
using OregonNexus.Broker.Connector.PayloadContentTypes;
using OregonNexus.Broker.Data;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Domain.Specifications;
using OregonNexus.Broker.Service;
using OregonNexus.Broker.SharedKernel;
using OregonNexus.Broker.Web.Constants.DesignSystems;
using OregonNexus.Broker.Web.Helpers;
using OregonNexus.Broker.Web.Models;
using OregonNexus.Broker.Web.Services.PayloadContents;
using static System.Net.Mime.MediaTypeNames;

namespace OregonNexus.Broker.Web.Controllers;

[Authorize(Policy = "SuperAdmin")]
public class SettingsController : AuthenticatedController
{
    private readonly ConnectorLoader _connectorLoader;
    private readonly ConfigurationSerializer _configurationSerializer;
    private readonly PayloadSerializer _payloadSerializer;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRepository<EducationOrganizationConnectorSettings> _repo;
    private readonly IRepository<EducationOrganizationPayloadSettings> _educationOrganizationPayloadSettings;
    private readonly PayloadContentTypeService _payloadContentTypeService;
    
    private readonly FocusHelper _focusHelper;

    private Guid? _focusedDistrictEdOrg { get; set; }

    public SettingsController(
        IHttpContextAccessor httpContextAccessor,
        ConnectorLoader connectorLoader, 
        IServiceProvider serviceProvider, 
        IRepository<EducationOrganizationConnectorSettings> repo, 
        FocusHelper focusHelper, 
        ConfigurationSerializer configurationSerializer, 
        IRepository<EducationOrganizationPayloadSettings> educationOrganizationPayloadSettings,
        PayloadSerializer payloadSerializer,
        PayloadContentTypeService payloadContentTypeService
        ) : base(httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(connectorLoader);

        _configurationSerializer = configurationSerializer;
        _connectorLoader = connectorLoader;
        _serviceProvider = serviceProvider;
        _repo = repo;
        _focusHelper = focusHelper;
        _educationOrganizationPayloadSettings = educationOrganizationPayloadSettings;
        _payloadSerializer = payloadSerializer;
        _payloadContentTypeService = payloadContentTypeService;
    }

    public async Task<IActionResult> Index()
    {
        if (await FocusedToDistrict() is not null) return View();
        
        var connectors = _connectorLoader.Connectors;
        var payloads = _connectorLoader.Payloads;

        var settingsViewModel = new SettingsViewModel() {
            ConnectorTypes = connectors,
            PayloadTypes = payloads
        };

        return View(settingsViewModel);
    }

    [HttpGet("/Settings/Configuration/{assembly}")]
    public async Task<IActionResult> Configuration(string assembly)
    {
        if (await FocusedToDistrict() is not null) return await FocusedToDistrict();

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
    }

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

    [HttpPost("/Settings/Configuration/{assembly}")]
    public async Task<IActionResult> Update(IFormCollection collection)
    {
        if (await FocusedToDistrict() is not null) return await FocusedToDistrict();

        var assemblyQualifiedName = collection["ConnectorConfigurationType"];

        // Get Connector Config Type
        Type connectorConfigType = Type.GetType(assemblyQualifiedName!, true)!;

        var iconfigModel = await _configurationSerializer.DeseralizeAsync(connectorConfigType, _focusedDistrictEdOrg.Value);

        // Loop through properties and set from form
        foreach(var prop in iconfigModel.GetType().GetProperties())
        {
            prop.SetValue(iconfigModel, collection[prop.Name].ToString());
        }

        await _configurationSerializer.SerializeAndSaveAsync(iconfigModel, _focusedDistrictEdOrg.Value);

        TempData[VoiceTone.Positive] = $"Updated Settings.";

        return RedirectToAction("Configuration", new { assembly = connectorConfigType.Assembly.GetName().Name });
    }

    [HttpGet("/Settings/Mapping")]
    public async Task<IActionResult> Mapping()
    {
        if (await FocusedToDistrict() is not null) return await FocusedToDistrict();
        
        return View(new {});
    }

    private async Task<IActionResult?> FocusedToDistrict()
    {
        if (_focusedDistrictEdOrg == null)
        {
            _focusedDistrictEdOrg = await _focusHelper.CurrentDistrictEdOrgFocus();
        }
        
        if (!_focusedDistrictEdOrg.HasValue)
        {
            TempData[VoiceTone.Critical] = $"Must be focused to a district.";
            return RedirectToAction(nameof(Index));
        }

        return null;
    }
}
