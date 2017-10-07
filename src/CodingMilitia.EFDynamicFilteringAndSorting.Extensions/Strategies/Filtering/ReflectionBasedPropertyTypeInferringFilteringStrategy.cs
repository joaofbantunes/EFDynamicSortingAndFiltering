using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CodingMilitia.EFDynamicFilteringAndSorting.Extensions.Factory.Filtering
{
    internal class ReflectionBasedPropertyTypeInferringFilteringStrategy : IFilteringStrategy
    {
        private readonly Dictionary<Type, Dictionary<string, LambdaExpression>> _propertyAccessorCache;
        private readonly Dictionary<Type, Dictionary<Type, MethodInfo>> _comparerCache;

        private readonly IDictionary<Type, Func<string, object>> _valueParserMap;

        private readonly bool _enableReflectionCaching;
        public ReflectionBasedPropertyTypeInferringFilteringStrategy(IDictionary<Type, Func<string, object>> valueParserMap, bool enableReflectionCaching = true)
        {
            _valueParserMap = valueParserMap;
            _enableReflectionCaching = enableReflectionCaching;
            _propertyAccessorCache = new Dictionary<Type, Dictionary<string, LambdaExpression>>();
            _comparerCache = new Dictionary<Type, Dictionary<Type, MethodInfo>>();
        }

        private class PropertyWrapper
        {
            public object Value { get; }

            public PropertyWrapper(object value)
            {
                Value = value;
            }
        }

        public IQueryable<TEntity> Filter<TEntity>(IQueryable<TEntity> query, params Filter[] filters)
        {
            return filters.Aggregate(query, (q, f) => q.Where(GetComparer<TEntity>(f)));
        }

        private Expression<Func<TEntity, bool>> GetComparer<TEntity>(Filter filter)
        {
            var entityType = typeof(TEntity);
            var propertyType = entityType.GetProperty(filter.PropertyName, BindingFlags.Instance | BindingFlags.Public).PropertyType;
            
            MethodInfo genericInnerGetComparer = GetInnerComparerMethodInfoForGenericTypes(entityType, propertyType);
            
            return genericInnerGetComparer.Invoke(
                this,
                new object[] { filter.PropertyName, filter.Type, ParseValues(filter.Values, propertyType) }
            ) as Expression<Func<TEntity, bool>>;
        }

        private MethodInfo GetInnerComparerMethodInfoForGenericTypes(Type entityType, Type propertyType)
        {
            if (!_comparerCache.TryGetValue(entityType, out var propertyEntries))
            {
                propertyEntries = new Dictionary<Type, MethodInfo>();

                if (_enableReflectionCaching)
                {
                    _comparerCache.Add(entityType, propertyEntries);
                }
            }
            if (!propertyEntries.TryGetValue(propertyType, out var propertyTypeComparer))
            {
                propertyTypeComparer = typeof(ReflectionBasedPropertyTypeInferringFilteringStrategy)
                    .GetMethod(nameof(InnerGetComparer), BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(new[] { entityType, propertyType });

                if (_enableReflectionCaching)
                {
                    propertyEntries.Add(propertyType, propertyTypeComparer);
                }
            }
            return propertyTypeComparer;
        }

        private PropertyWrapper[] ParseValues(IEnumerable<string> values, Type propertyType)
        {
            if (_valueParserMap.TryGetValue(propertyType, out var parser))
            {
                return values.Select(v => new PropertyWrapper(parser(v))).ToArray();
            }
            throw new InvalidOperationException("Cannot parse provided value(s).");
        }

        private Expression<Func<TEntity, bool>> InnerGetComparer<TEntity, TProperty>(string propertyName, FilterType filterType, PropertyWrapper[] values)
        {
            var selector = GetPropertyAccessor<TEntity, TProperty>(propertyName);
            var propertyRef = selector.Body;
            var parameter = selector.Parameters[0];

            Expression<Func<TEntity, bool>> comparer = null;

            switch (filterType)
            {
                case FilterType.Equals:
                case FilterType.NotEquals:
                    comparer = BuildEqualityExpression<TEntity>(values, propertyRef, parameter, filterType);
                    break;
                case FilterType.Range:
                    comparer = BuildRangeExpression<TEntity>(values, propertyRef, parameter);
                    break;
                case FilterType.Contains:
                    comparer = BuildContainsExpression<TEntity>(values, propertyRef, parameter);
                    break;
                case FilterType.Less:
                case FilterType.LessOrEqual:
                case FilterType.Greater:
                case FilterType.GreaterOrEqual:
                    comparer = BuildSimpleComparisonExpression<TEntity>(values, propertyRef, parameter, filterType);
                    break;
            }
            return comparer;
        }

        private LambdaExpression GetPropertyAccessor<TEntity, TProperty>(string propertyName)
        {
            var entityType = typeof(TEntity);
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
                    baseCreatePropertyAccessor.MakeGenericMethod(new[] { entityType, typeof(TProperty) });
                concretePropertyAccessor = genericCreatePropertyAccessor.Invoke(null, new[] { propertyName }) as LambdaExpression;
                if (_enableReflectionCaching)
                {
                    propertyEntries.Add(propertyName, concretePropertyAccessor);
                }
            }
            return concretePropertyAccessor;
        }


        private static Expression<Func<TEntity, bool>> BuildEqualityExpression<TEntity>(PropertyWrapper[] values,
            Expression propertyRef, ParameterExpression parameter, FilterType filterType)
        {
            if (values.Any(v => v == null))
            {
                throw new ArgumentException("Equality expression doesn't accept null arguments for non-nullable properties.", nameof(values));
            }

            BinaryExpression equalityAccumulator = null;
            foreach (var value in values.Select(v => v.Value))
            {
                var constantRef = Expression.Constant(value);
                BinaryExpression equalityExpression = null;
                switch (filterType)
                {
                    case FilterType.Equals:
                        equalityExpression = Expression.Equal(propertyRef, constantRef);
                        equalityAccumulator = equalityAccumulator != null ? Expression.Or(equalityAccumulator, equalityExpression) : equalityExpression;
                        break;
                    case FilterType.NotEquals:
                        equalityExpression = Expression.NotEqual(propertyRef, constantRef);
                        equalityAccumulator = equalityAccumulator != null ? Expression.And(equalityAccumulator, equalityExpression) : equalityExpression;
                        break;
                    default:
                        throw new ArgumentException($"Unexpected FilterType {filterType}", "filterType");
                }

            }
            if (equalityAccumulator == null)
            {
                throw new ArgumentException("No values were provided", "values");
            }
            return Expression.Lambda<Func<TEntity, bool>>(equalityAccumulator, parameter);
        }

        private static Expression<Func<TEntity, bool>> BuildRangeExpression<TEntity>(PropertyWrapper[] values,
          Expression propertyRef, ParameterExpression parameter)
        {
            BinaryExpression rangeAccumulator = null;
            if (values[0] != null)
            {
                var lowerBound = Expression.Constant(values[0].Value);
                rangeAccumulator = Expression.GreaterThanOrEqual(propertyRef, lowerBound);
            }
            if (values[1] != null)
            {
                var upperBound = Expression.Constant(values[1].Value);
                var upperExpression = Expression.LessThanOrEqual(propertyRef, upperBound);
                rangeAccumulator = rangeAccumulator != null
                    ? Expression.And(rangeAccumulator, upperExpression)
                    : upperExpression;
            }
            if (rangeAccumulator == null)
            {
                throw new ArgumentException("No values were provided", "values");
            }
            return Expression.Lambda<Func<TEntity, bool>>(rangeAccumulator, parameter);
        }

        private static Expression<Func<TEntity, bool>> BuildContainsExpression<TEntity>(
            PropertyWrapper[] values, Expression propertyRef, ParameterExpression parameter)
        {
            if (values.Any(v => v == null))
            {
                throw new ArgumentException("Range expression doesn't accept null arguments.", nameof(values));
            }

            //TODO: use typeof(TProperty)?
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            Expression containsAccumulator = null;
            foreach (var value in values.Select(v => v.Value))
            {
                var constantRef = Expression.Constant(value);
                var containsExpression = Expression.Call(propertyRef, containsMethod, constantRef);
                containsAccumulator = containsAccumulator != null ? Expression.Or(containsAccumulator, containsExpression) : (Expression)containsExpression;
            }

            if (containsAccumulator == null)
            {
                throw new ArgumentException("No values were provided", "values");
            }

            return Expression.Lambda<Func<TEntity, bool>>(containsAccumulator, parameter);
        }

        private static Expression<Func<TEntity, bool>> BuildSimpleComparisonExpression<TEntity>(
            PropertyWrapper[] values,
            Expression propertyRef, ParameterExpression parameter, FilterType filterType)
        {

            if (values.Any(v => v == null))
            {
                throw new ArgumentException($"{filterType} comparer expression doesn't accept null arguments.", nameof(values));
            }

            var constantRef = Expression.Constant(values.Single().Value);
            BinaryExpression comparisonExpression = null;
            switch (filterType)
            {
                case FilterType.Less:
                    comparisonExpression = Expression.LessThan(propertyRef, constantRef);
                    break;
                case FilterType.LessOrEqual:
                    comparisonExpression = Expression.LessThanOrEqual(propertyRef, constantRef);
                    break;
                case FilterType.Greater:
                    comparisonExpression = Expression.GreaterThan(propertyRef, constantRef);
                    break;
                case FilterType.GreaterOrEqual:
                    comparisonExpression = Expression.GreaterThanOrEqual(propertyRef, constantRef);
                    break;
                default:
                    throw new ArgumentException("Unexpected FilterType " + filterType, "filterType");
            }

            return Expression.Lambda<Func<TEntity, bool>>(comparisonExpression, parameter);
        }
    }
}
