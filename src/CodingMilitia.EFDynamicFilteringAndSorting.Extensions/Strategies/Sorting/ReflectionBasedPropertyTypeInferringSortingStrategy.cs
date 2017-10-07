using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CodingMilitia.EFDynamicFilteringAndSorting.Extensions.Strategies.Sorting
{
    internal class ReflectionBasedPropertyTypeInferringSortingStrategy : ISortingStrategy
    {
        private readonly Dictionary<Type, Dictionary<string, LambdaExpression>> _propertyAccessorCache;
        private readonly bool _enableReflectionCaching;

        public ReflectionBasedPropertyTypeInferringSortingStrategy(bool enableReflectionCaching = true)
        {
            _enableReflectionCaching = enableReflectionCaching;
            _propertyAccessorCache = new Dictionary<Type, Dictionary<string, LambdaExpression>>();
        }

        public IQueryable<TEntity> Sort<TEntity>(IQueryable<TEntity> items, SortCriteria[] sortCriteria)
        {
            if (sortCriteria == null || sortCriteria.Length == 0)
                return items;

            var ordered = InnerSort(items, sortCriteria[0]);
            for (var i = 1; i < sortCriteria.Length; ++i)
            {
                ordered = InnerSort(ordered, sortCriteria[i]);
            }
            return ordered;
        }

        private IOrderedQueryable<TEntity> InnerSort<TEntity>(IQueryable<TEntity> items, SortCriteria criteria)
        {
            IOrderedQueryable<TEntity> ordered = null;

            var entityType = typeof(TEntity);
            var propertyType = entityType.GetProperty(criteria.PropertyName, BindingFlags.Instance | BindingFlags.Public).PropertyType;

            LambdaExpression selector = GetPropertyAccessor(entityType, propertyType, criteria.PropertyName);
            Type[] typeArgs = new Type[] { entityType, propertyType };

            var mc = Expression.Call(typeof(Queryable), GetSortMethod(items, criteria), typeArgs, items.Expression, selector);

            ordered = items.Provider.CreateQuery(mc) as IOrderedQueryable<TEntity>;

            return ordered;
        }

        private LambdaExpression GetPropertyAccessor(Type entityType, Type propertyType, string propertyName)
        {
            if (!_propertyAccessorCache.TryGetValue(entityType, out var propertyEntries))
            {
                propertyEntries = new Dictionary<string, LambdaExpression>();
                if (_enableReflectionCaching)
                {
                    _propertyAccessorCache.Add(entityType, propertyEntries);
                }
            }
            if (!propertyEntries.TryGetValue(propertyName, out var concretePropertyAccessor))
            {
                var baseCreatePropertyAccessor = typeof(ExpressionHelper).GetMethod(nameof(ExpressionHelper.CreatePropertyAccessor),
                            BindingFlags.NonPublic | BindingFlags.Static);
                var genericCreatePropertyAccessor =
                    baseCreatePropertyAccessor.MakeGenericMethod(new[] { entityType, propertyType });
                concretePropertyAccessor = genericCreatePropertyAccessor.Invoke(null, new[] { propertyName }) as LambdaExpression;
                if (_enableReflectionCaching)
                {
                    propertyEntries.Add(propertyName, concretePropertyAccessor);
                }
            }
            return concretePropertyAccessor;
        }

        private static string GetSortMethod<TEntity>(IQueryable<TEntity> items, SortCriteria criteria)
        {
            if (items.GetType().IsAssignableFrom(typeof(IOrderedQueryable<TEntity>)))
            {
                return criteria.Direction == SortDirection.Descending ? "ThenByDescending" : "ThenBy";
            }
            else
            {
                return criteria.Direction == SortDirection.Descending ? "OrderByDescending" : "OrderBy";
            }
        }
    }
}