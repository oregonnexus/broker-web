#nullable disable
using Newtonsoft.Json;
namespace OregonNexus.Broker.Web.Models.JsonDocuments;

public class SynergyJsonModel
{
    [JsonProperty("OregonNexus.Broker.Connector.Edupoint.Synergy.Student")]
    public SynergyStudentJsonModel Student { get; set; }
}

