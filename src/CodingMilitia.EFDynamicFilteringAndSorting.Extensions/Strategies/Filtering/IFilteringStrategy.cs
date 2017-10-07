using System;
using System.Linq;
using System.Linq.Expressions;

namespace CodingMilitia.EFDynamicFilteringAndSorting.Extensions.Factory.Filtering
{
    internal interface IFilteringStrategy
    {
         IQueryable<TEntity> Filter<TEntity>(IQueryable<TEntity> query, params Filter[] filters);
    }
}