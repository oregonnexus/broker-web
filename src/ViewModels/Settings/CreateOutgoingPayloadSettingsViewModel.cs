using System.ComponentModel.DataAnnotations;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Web.Constants.DesignSystems;
using OregonNexus.Broker.Web.Extensions.RequestStatuses;
using OregonNexus.Broker.Web.Models.JsonDocuments;

namespace OregonNexus.Broker.Web.ViewModels.Settings;

public class CreateOutgoingPayloadSettingsViewModel
{
    public string? StudentLookupConnector { get; set; }
}