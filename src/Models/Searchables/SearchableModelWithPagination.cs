using OregonNexus.Broker.Web.Models.Paginations;

namespace OregonNexus.Broker.Web.Models.Searchables
{
	public class SearchableModelWithPagination : PaginationModel
	{
        public string SearchBy { get; set; } = string.Empty;
    }
}

