using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CodingMilitia.EFDynamicFilteringAndSorting.Extensions.Factory.Sorting
{
    internal class HardcodedPropertyTypeInferringSortingStrategy : ISortingStrategy
    {
        public IQueryable<TEntity> Sort<TEntity>(IQueryable<TEntity> items, SortCriteria[] sortCriteria)
        {
            if (sortCriteria == null || sortCriteria.Length == 0)
                return items;

            var ordered = InnerSort(items, sortCriteria[0], true);
            for (var i = 1; i < sortCriteria.Length; ++i)
            {
                ordered = InnerSort(ordered, sortCriteria[i], false);
            }
            return ordered;
        }
        private static IOrderedQueryable<TEntity> InnerSort<TEntity>(IQueryable<TEntity> items, SortCriteria criteria, bool isMainSortProperty)
        {
            IOrderedQueryable<TEntity> ordered = null;

            var propertyType =
                typeof(TEntity).GetProperty(criteria.PropertyName, BindingFlags.Instance | BindingFlags.Public).PropertyType;

            if (propertyType.IsAssignableFrom(typeof(string)))
            {
                var propertyAccessor = ExpressionHelper.CreatePropertyAccessor<TEntity, string>(criteria.PropertyName);
                ordered = isMainSortProperty ? OrderBy(items, propertyAccessor, criteria.Direction) : ThenBy((items as IOrderedQueryable<TEntity>), propertyAccessor, criteria.Direction);
            }
            else if (propertyType.IsAssignableFrom(typeof(int?)))
            {
                var propertyAccessor = ExpressionHelper.CreatePropertyAccessor<TEntity, int?>(criteria.PropertyName);
                ordered = isMainSortProperty ? OrderBy(items, propertyAccessor, criteria.Direction) : ThenBy((items as IOrderedQueryable<TEntity>), propertyAccessor, criteria.Direction);
            }
            else if (propertyType.IsAssignableFrom(typeof(int)))
            {
                var propertyAccessor = ExpressionHelper.CreatePropertyAccessor<TEntity, int>(criteria.PropertyName);
                ordered = isMainSortProperty ? OrderBy(items, propertyAccessor, criteria.Direction) : ThenBy((items as IOrderedQueryable<TEntity>), propertyAccessor, criteria.Direction);
            }
            else if (propertyType.IsAssignableFrom(typeof(DateTime?)))
            {
                var propertyAccessor = ExpressionHelper.CreatePropertyAccessor<TEntity, DateTime?>(criteria.PropertyName);
                ordered = isMainSortProperty ? OrderBy(items, propertyAccessor, criteria.Direction) : ThenBy((items as IOrderedQueryable<TEntity>), propertyAccessor, criteria.Direction);
            }
            else if (propertyType.IsAssignableFrom(typeof(DateTime)))
            {
                var propertyAccessor = ExpressionHelper.CreatePropertyAccessor<TEntity, DateTime>(criteria.PropertyName);
                ordered = isMainSortProperty ? OrderBy(items, propertyAccessor, criteria.Direction) : ThenBy((items as IOrderedQueryable<TEntity>), propertyAccessor, criteria.Direction);
            }

            if (ordered == null)
                throw new ArgumentException(string.Format("Unable to sort by the requested criteria: {0} - {1}", criteria.PropertyName, criteria.Direction), "criteria");

            return ordered;
        }

        private static IOrderedQueryable<T> OrderBy<T, TProperty>(IQueryable<T> items, Expression<Func<T, TProperty>> expression, SortDirection direction)
        {
            switch (direction)
            {
                case SortDirection.Ascending:
                default:
                    return items.OrderBy(expression);
                case SortDirection.Descending:
                    return items.OrderByDescending(expression);
            }
        }

        private static IOrderedQueryable<T> ThenBy<T, TProperty>(IOrderedQueryable<T> items, Expression<Func<T, TProperty>> expression, SortDirection direction)
        {
            switch (direction)
            {
                case SortDirection.Ascending:
                default:
                    return items.ThenBy(expression);
                case SortDirection.Descending:
                    return items.ThenByDescending(expression);
            }
        }
    }
}