using System.ComponentModel.DataAnnotations;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Web.Constants.DesignSystems;
using OregonNexus.Broker.Web.Extensions.RequestStatuses;
using OregonNexus.Broker.Web.Models.JsonDocuments;

namespace OregonNexus.Broker.Web.ViewModels.Settings;

public class PayloadSettingsViewModel
{
    public string FullName { get; set; } = default!;

    public string DisplayName { get; set; } = default!;

    public string? Configuration { get; set; }
}