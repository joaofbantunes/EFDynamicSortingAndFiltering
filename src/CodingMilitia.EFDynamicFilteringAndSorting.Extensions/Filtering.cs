using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CodingMilitia.EFDynamicFilteringAndSorting.Extensions.Factory.Filtering;

namespace CodingMilitia.EFDynamicFilteringAndSorting.Extensions
{
    public enum FilteringExpressionStrategy { Hardcoded, Reflection }
    public static class Filtering
    {
        private static IFilteringStrategy Strategy;

        private static readonly Dictionary<Type, Func<string, object>> DefaultValueParserMap = new Dictionary<Type, Func<string, object>> 
        {
            [typeof(string)] = v => v,
            [typeof(int)] = v => int.Parse(v),
            [typeof(int?)] = v => !string.IsNullOrEmpty(v) ? int.Parse(v) : (int?)null,
            [typeof(DateTime)] = v => DateTime.Parse(v, CultureInfo.InvariantCulture),
            [typeof(Guid?)] = v => !string.IsNullOrEmpty(v) ? DateTime.Parse(v, CultureInfo.InvariantCulture) : (DateTime?)null,
            [typeof(Guid)] = v => Guid.Parse(v),
            [typeof(Guid?)] = v => !string.IsNullOrEmpty(v) ? Guid.Parse(v) : (Guid?)null,
        };

        static Filtering()
        {
            Strategy = new ReflectionBasedPropertyTypeInferringFilteringStrategy(DefaultValueParserMap, true);
        }

        public static IQueryable<TEntity> Filter<TEntity>(this IQueryable<TEntity> query, params Filter[] filters)
        {
            return Strategy.Filter(query, filters);
        }

        public static void SetupTestingEnvironment(FilteringExpressionStrategy strategy, bool enableReflectionCache)
        {
            switch (strategy)
            {
                case FilteringExpressionStrategy.Hardcoded:
                    Strategy = new HardcodedPropertyTypeInferringFilteringStrategy();
                    break;
                case FilteringExpressionStrategy.Reflection:
                    Strategy = new ReflectionBasedPropertyTypeInferringFilteringStrategy(DefaultValueParserMap, enableReflectionCache);
                    break;
                default:
                    throw new NotImplementedException($"The requested strategy {strategy} is not yet implemented.");
            }
        }
    }
}