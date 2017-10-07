using System.Linq;

namespace CodingMilitia.EFDynamicFilteringAndSorting.Extensions.Factory.Sorting
{
    internal interface ISortingStrategy
    {
         IQueryable<TEntity> Sort<TEntity>(IQueryable<TEntity> query, SortCriteria[] sortCriteria);
    }
}