using System.Linq.Expressions;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Web.Models.Searchables;
#nullable disable

namespace OregonNexus.Broker.Web.Models.OutgoingRequests;

public class EducationOrganizationRequestModel : SearchableModelWithPagination
{
    public string District { get; set; }
    public string Name { get; set; }
    public string Number { get; set; }
    public string Type { get; set; }

    public Expression<Func<EducationOrganization, object>> BuildSortExpression()
    {
        Expression<Func<EducationOrganization, object>> sortExpression = null;
        var sortBy = SortBy.ToLower();
        sortExpression = sortBy switch
        {
            "district" => educationOrganization => educationOrganization.ParentOrganization.Name,
            "name" => educationOrganization => educationOrganization.Name,
            "number" => educationOrganization => educationOrganization.Number,
            "type" => educationOrganization => educationOrganization.EducationOrganizationType,
            _ => educationOrganization => educationOrganization.Id,
        };
        return sortExpression;
    }

    public List<Expression<Func<EducationOrganization, bool>>> BuildSearchExpressions()
    {
        var searchExpressions = new List<Expression<Func<EducationOrganization, bool>>>();

        if (!string.IsNullOrWhiteSpace(SearchBy))
        {
            searchExpressions.Add(
                educationOrganization => educationOrganization.ParentOrganization.Name
                    .ToLower()
                    .Contains(SearchBy.ToLower())
                || educationOrganization.Name
                    .ToLower()
                    .Contains(SearchBy.ToLower())
                || educationOrganization.Number.ToLower().Contains(SearchBy.ToLower())
            );
        }

        if (!string.IsNullOrWhiteSpace(District))
        {
            searchExpressions.Add(
                educationOrganization => educationOrganization.ParentOrganization.Name
                    .ToLower()
                    .Contains(District.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(Name))
        {
            searchExpressions.Add(educationOrganization => educationOrganization.Name
                .ToLower()
                .Contains(Name.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(Number))
        {
            searchExpressions.Add(educationOrganization => educationOrganization.Number
                .ToLower()
                .Contains(Number.ToLower()));
        }

        if (Enum.TryParse<EducationOrganizationType>(Type, out var type))
        {
            searchExpressions.Add(educationOrganization => educationOrganization.EducationOrganizationType == type);
        }

        return searchExpressions;
    }
}