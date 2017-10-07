using System.Linq;

namespace CodingMilitia.EFDynamicFilteringAndSorting.Extensions.Strategies.Sorting
{
    internal interface ISortingStrategy
    {
         IQueryable<TEntity> Sort<TEntity>(IQueryable<TEntity> query, SortCriteria[] sortCriteria);
    }
}