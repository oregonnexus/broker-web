namespace OregonNexus.Broker.Web.Models.JsonDocuments;
#nullable disable
public class ResponseManifestJsonModel
{
    public string RequestId { get; set; }
    public string ResponseType { get; set; }
    public StudentJsonModel Student { get; set; }
    public SchoolJsonModel From { get; set; }
    public SchoolJsonModel To { get; set; }
    public string Note { get; set; }
    public List<string> Contents { get; set; }
}

