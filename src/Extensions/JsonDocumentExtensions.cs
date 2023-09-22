using System.Text.Json;

namespace OregonNexus.Broker.Web;

public static class JsonDocumentExtensions
{
    public static JsonDocument ToJsonDocument(this object model)
    {
        string jsonString = JsonSerializer.Serialize(model);

        return JsonDocument.Parse(jsonString);
    }
}
