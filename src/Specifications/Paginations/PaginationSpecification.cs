using System.Linq.Expressions;
using Ardalis.Specification;

namespace OregonNexus.Broker.Web.Specifications.Paginations
{
    public class SearchableWithPaginationSpecification<T> : Specification<T>
    {
        public int PageNumber { get; }
        public int PageSize { get; }
        public bool Ascending { get; }
        public Expression<Func<T, object>>? SortExpression { get; }
        public List<Expression<Func<T, bool>>> SearchExpressions { get; }

        private SearchableWithPaginationSpecification(
            int pageNumber,
            int pageSize,
            bool ascending,
            Expression<Func<T, object>>? sortExpression,
            List<Expression<Func<T, bool>>> searchExpressions,
            Action<ISpecificationBuilder<T>>? includeAction
        )
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            Ascending = ascending;
            SortExpression = sortExpression;
            SearchExpressions = searchExpressions;

            if(pageSize > 0)
            {
                int skip = (pageNumber - 1) * pageSize;
                Query.Skip(skip);
                Query.Take(pageSize);
            }

            if (sortExpression != null)
            {
                if (Ascending)
                    Query.OrderBy(sortExpression!);
                else
                    Query.OrderByDescending(sortExpression!);
            }

            foreach (var searchExpression in searchExpressions)
            {
                Query.Where(searchExpression);
            }

            includeAction?.Invoke(Query);
        }

        public class Builder
        {
            private readonly int _pageNumber;
            private readonly int _pageSize;
            private bool _ascending;
            private Expression<Func<T, object>>? _sortExpression;
            private List<Expression<Func<T, bool>>> _searchExpressions;
            private Action<ISpecificationBuilder<T>>? _includeAction;

            public Builder(
                int pageNumber,
                int pageSize)
            {
                _pageNumber = pageNumber;
                _pageSize = pageSize;
                _searchExpressions = new List<Expression<Func<T, bool>>>();
            }

            public Builder WithAscending(bool ascending)
            {
                _ascending = ascending;
                return this;
            }

            public Builder WithSortExpression(Expression<Func<T, object>>? sortExpression)
            {
                _sortExpression = sortExpression;
                return this;
            }

            public Builder WithSearchExpression(Expression<Func<T, bool>> searchExpression)
            {
                _searchExpressions.Add(searchExpression);
                return this;
            }

            public Builder WithSearchExpressions(List<Expression<Func<T, bool>>> searchExpressions)
            {
                _searchExpressions.AddRange(searchExpressions);
                return this;
            }

            public Builder WithIncludeEntities(Action<ISpecificationBuilder<T>> includeAction)
            {
                _includeAction = includeAction;
                return this;
            }

            public SearchableWithPaginationSpecification<T> Build()
            {
                return new SearchableWithPaginationSpecification<T>(
                    _pageNumber,
                    _pageSize,
                    _ascending,
                    _sortExpression,
                    _searchExpressions,
                    _includeAction
                );
            }
        }
    }
}
