using System.ComponentModel;
using System.Dynamic;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using OregonNexus.Broker.Connector;
using OregonNexus.Broker.Connector.Configuration;
using OregonNexus.Broker.Connector.Payload;
using OregonNexus.Broker.Data;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Domain.Specifications;
using OregonNexus.Broker.SharedKernel;
using OregonNexus.Broker.Web.Constants.DesignSystems;
using OregonNexus.Broker.Web.Helpers;
using OregonNexus.Broker.Web.Models;

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
        PayloadSerializer payloadSerializer
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

        var payloadAssembly = _connectorLoader.Payloads.Where(x => x.FullName == payload).FirstOrDefault();
        ArgumentException.ThrowIfNullOrEmpty(payload);
        var payloads = _connectorLoader.GetPayloads(payloadAssembly!.Assembly);

        var contentTypes = _connectorLoader.GetContentTypes();

        // Make drop down of content types
        var contentTypesSelect = new List<SelectListItem>();

        if (contentTypes?.Count > 0)
        {
            foreach(var contentType in contentTypes)
            {
                contentTypesSelect.Add(new SelectListItem()
                {
                    Value = contentType.Name,
                    Text = contentType.Name
                });
            }
        }

        ViewBag.ContentTypesSelect = contentTypesSelect;

        var forms = new List<dynamic>();
        var payloadDirection = PayloadDirection.Outgoing;

        // foreach (var payloadType in payloads)
        // {
        //     var payloadModel = await _payloadSerializer.DeseralizeAsync(payloadType, payloadDirection, _focusedDistrictEdOrg.Value);
        //     var displayName = (DisplayNameAttribute)payloadType.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DisplayNameAttribute)).FirstOrDefault()!;

            // forms.Add(
            //     new
            //     {
            //         displayName = displayName.DisplayName,
            //         html = ModelFormBuilderHelper.HtmlForModel(payloadModel, payloadDirection)
            //     }
            // );
        // }

        return View(forms);
    }

    public class TempStudentCumulativePayloadSettings
    {
        public string? Students { get; set; }
        public string? StudentAssessments { get; set; }
        public string? Grades { get; set; }
        public string? CourseTranscripts { get; set; }
        public string? ProgramAssociations { get; set; }
        public string? StudentSectionAssociations { get; set; }
        public string? DisciplineActions { get; set; }
        public string? DisciplineIncidents { get; set; }
    }

    [HttpPost("/Settings/OutgoingPayload/{payload}")]
    public async Task<IActionResult> UpdateOutgoingPayload(
        [FromRoute] string payload,
        [FromForm] TempStudentCumulativePayloadSettings settings)
    {
        if (await FocusedToDistrict() is not null) return await FocusedToDistrict();

        var userId = Guid.Parse(User.FindFirstValue(claimType: ClaimTypes.NameIdentifier)!);
        var today = DateTime.UtcNow;

        var payloads = await _educationOrganizationPayloadSettings
            .ListAsync();

        var currentPayload = payloads.SingleOrDefault(p => p.Payload == payload && p.EducationOrganizationId == _focusedDistrictEdOrg!.Value);

        if (currentPayload is not null)
        {
            currentPayload.Settings = settings.ToJsonDocument();
            await _educationOrganizationPayloadSettings.UpdateAsync(currentPayload);
        }
        else
        {
            await _educationOrganizationPayloadSettings.AddAsync(new EducationOrganizationPayloadSettings()
            {
                EducationOrganizationId = _focusedDistrictEdOrg!.Value,
                PayloadDirection = PayloadDirection.Outgoing,
                Payload = payload,
                Settings = settings.ToJsonDocument()
            });
        }

        await _educationOrganizationPayloadSettings.SaveChangesAsync();

        return RedirectToAction("OutgoingPayload", new { payload = payload });
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
