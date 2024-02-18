using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Domain.Extensions;
using OregonNexus.Broker.Web.Constants.DesignSystems;
using System.Reflection;

namespace OregonNexus.Broker.Web.ViewModels.Mappings;

public class MappingViewModel
{
    public Guid? MappingId { get; set; }

    public List<Mapping>? RequestMappings { get; set; }

    public Mapping? Mapping { get; set; }

    public PropertyInfo[]? Properties { get; set; }

    public DisplayNameAttribute? ResolveMappingTypeDisplayName(string mappingTypeName)
    {
        var mappingType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => p.FullName == mappingTypeName).FirstOrDefault();
        return (DisplayNameAttribute)mappingType?.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DisplayNameAttribute)).FirstOrDefault()!;
    }

    public DisplayNameAttribute? GetPropertyDisplayName(PropertyInfo property)
    {
        return (DisplayNameAttribute)property?.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DisplayNameAttribute)).FirstOrDefault()!;
    }

    public DataType? GetPropertyDataType(PropertyInfo property)
    {
        return (DataType)property?.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DataType)).FirstOrDefault()!;
    }
}