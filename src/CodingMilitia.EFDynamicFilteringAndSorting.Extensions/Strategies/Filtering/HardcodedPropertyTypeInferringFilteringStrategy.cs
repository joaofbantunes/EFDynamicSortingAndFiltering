using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CodingMilitia.EFDynamicFilteringAndSorting.Extensions.Strategies.Filtering
{
    internal class HardcodedPropertyTypeInferringFilteringStrategy : IFilteringStrategy
    {
        public IQueryable<TEntity> Filter<TEntity>(IQueryable<TEntity> query, params Filter[] filters)
        {
            return filters.Aggregate(query, (q, f) => q.Where(GetComparer<TEntity>(f)));
        }

        private Expression<Func<TEntity, bool>> GetComparer<TEntity>(Filter filter)
        {
            var propertyType =
                typeof(TEntity).GetProperty(filter.PropertyName, BindingFlags.Instance | BindingFlags.Public).PropertyType;

            if (propertyType.IsAssignableFrom(typeof(string)))
            {
                return InnerGetComparer<TEntity, string, string>(filter.PropertyName, filter.Type, filter.Values);
            }
            if (propertyType.IsAssignableFrom(typeof(int)))
            {
                return InnerGetComparer<TEntity, int?, int>(filter.PropertyName, filter.Type, filter.Values.Select(v => !string.IsNullOrWhiteSpace(v) ? new int?(int.Parse(v)) : null));
            }
            if (propertyType.IsAssignableFrom(typeof(DateTime?)))
            {
                return InnerGetComparer<TEntity, DateTime?, DateTime?>(filter.PropertyName, filter.Type,
                    filter.Values.Select(v => !string.IsNullOrWhiteSpace(v) ? new DateTime?(DateTime.Parse(v, CultureInfo.InvariantCulture)) : null));
            }
            if (propertyType.IsAssignableFrom(typeof(DateTime)))
            {
                return InnerGetComparer<TEntity, DateTime?, DateTime>(filter.PropertyName, filter.Type,
                    filter.Values.Select(v => !string.IsNullOrWhiteSpace(v) ? new DateTime?(DateTime.Parse(v, CultureInfo.InvariantCulture)) : null));
            }
            throw new ArgumentException("Unexpected filter type " + filter.Type, "filter");
        }

        private static Expression<Func<TEntity, bool>> InnerGetComparer<TEntity, TPropertyPassed, TProperty>(string propertyName, FilterType filterType, IEnumerable<TPropertyPassed> values)
        {
            var valuesArray = values.ToArray();
            var selector = ExpressionHelper.CreatePropertyAccessor<TEntity, TProperty>(propertyName);
            var propertyRef = selector.Body;
            var parameter = selector.Parameters[0];

            Expression<Func<TEntity, bool>> comparer = null;

            switch (filterType)
            {
                case FilterType.Equals:
                case FilterType.NotEquals:
                    comparer = BuildEqualityExpression<TEntity, TPropertyPassed>(valuesArray, propertyRef, parameter, filterType);
                    break;
                case FilterType.Range:
                    comparer = BuildRangeExpression<TEntity, TPropertyPassed>(valuesArray, propertyRef, parameter);
                    break;
                case FilterType.Contains:
                    comparer = BuildContainsExpression<TEntity, TPropertyPassed>(valuesArray, propertyRef, parameter);
                    break;
                case FilterType.Less:
                case FilterType.LessOrEqual:
                case FilterType.Greater:
                case FilterType.GreaterOrEqual:
                    comparer = BuildSimpleComparisonExpression<TEntity, TPropertyPassed>(valuesArray, propertyRef, parameter, filterType);
                    break;
            }
            return comparer;
        }


        private static Expression<Func<TEntity, bool>> BuildEqualityExpression<TEntity, TPropertyPassed>(TPropertyPassed[] values,
            Expression propertyRef, ParameterExpression parameter, FilterType filterType)
        {
            BinaryExpression equalityAccumulator = null;
            foreach (var value in values)
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
                        throw new ArgumentException("Unexpected FilterType " + filterType, "filterType");
                }

            }
            if (equalityAccumulator == null)
            {
                throw new ArgumentException("No values were provided", "values");
            }
            return Expression.Lambda<Func<TEntity, bool>>(equalityAccumulator, parameter);
        }


        private static Expression<Func<TEntity, bool>> BuildRangeExpression<TEntity, TPropertyPassed>(TPropertyPassed[] values,
          Expression propertyRef, ParameterExpression parameter)
        {
            BinaryExpression rangeAccumulator = null;
            if (values[0] != null)
            {
                var lowerBound = Expression.Constant(values[0]);
                rangeAccumulator = Expression.GreaterThanOrEqual(propertyRef, lowerBound);
            }
            if (values[1] != null)
            {
                var upperBound = Expression.Constant(values[1]);
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

        private static Expression<Func<TEntity, bool>> BuildContainsExpression<TEntity, TPropertyPassed>(
            TPropertyPassed[] values, Expression propertyRef, ParameterExpression parameter)
        {

            var containsMethod = typeof(TPropertyPassed).GetMethod("Contains", new[] { typeof(string) });

            Expression containsAccumulator = null;
            foreach (var value in values)
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

        private static Expression<Func<TEntity, bool>> BuildSimpleComparisonExpression<TEntity, TPropertyPassed>(
            TPropertyPassed[] values,
            Expression propertyRef, ParameterExpression parameter, FilterType filterType)
        {

            var constantRef = Expression.Constant(values.Single());
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
