using System.Linq.Expressions;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Web.Models.Searchables;
#nullable disable

namespace OregonNexus.Broker.Web.Models.IncomingRequests;

public class IncomingRequestModel : SearchableModelWithPagination
{
    public string District { get; set; }
    public string School { get; set; }
    public string Student { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; }

    public Expression<Func<Request, object>> BuildSortExpression()
    {
        Expression<Func<Request, object>> sortExpression = null;
        var sortBy = SortBy.ToLower();
        sortExpression = sortBy switch
        {
            "district" => request => request.EducationOrganization.ParentOrganization.Name,
            "school" => request => request.EducationOrganization.Name,
            "student" => request => request.Student,
            "date" => request => request.CreatedAt,
            "status" => request => request.RequestStatus,
            _ => request => request.Id,
        };
        return sortExpression;
    }

    public List<Expression<Func<Request, bool>>> BuildSearchExpressions()
    {
        var searchExpressions = new List<Expression<Func<Request, bool>>>();

        if (!string.IsNullOrWhiteSpace(SearchBy))
        {
            searchExpressions.Add(
                request => request.EducationOrganization.ParentOrganization.Name
                    .ToLower()
                    .Contains(SearchBy.ToLower())
                || request.EducationOrganization.Name
                    .ToLower()
                    .Contains(SearchBy.ToLower())
                || request.Student.ToString().ToLower().Contains(SearchBy.ToLower())
            );
        }

        if (!string.IsNullOrWhiteSpace(District))
        {
            searchExpressions.Add(
                request => request.EducationOrganization.ParentOrganization.Name
                    .ToLower()
                    .Contains(District.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(School))
        {
            searchExpressions.Add(request => request.EducationOrganization.Name
                .ToLower()
                .Contains(School.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(Student))
        {
            searchExpressions.Add(request => request.Student.ToString()
                .ToLower()
                .Contains(Student.ToLower()));
        }

        if (StartDate.HasValue)
        {
            searchExpressions.Add(request => request.CreatedAt >= StartDate.Value);
        }

        if (EndDate.HasValue)
        {
            searchExpressions.Add(request => request.CreatedAt <= EndDate.Value);
        }

        if (Enum.TryParse<RequestStatus>(Status, out var requestStatus))
        {
            searchExpressions.Add(request => request.RequestStatus == requestStatus);
        }

        return searchExpressions;
    }
}
