#nullable disable
using Newtonsoft.Json;

namespace OregonNexus.Broker.Web.Models.JsonDocuments;

public class EdfiJsonModel
{
    [JsonProperty("OregonNexus.Broker.Connector.EdFiAlliance.EdFi.Student")]
    public EdfiStudentJsonModel Student { get; set; }
}

