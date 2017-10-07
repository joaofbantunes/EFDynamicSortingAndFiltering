using CodingMilitia.EFDynamicFilteringAndSorting.Extensions.Strategies.Sorting;
using System.Linq;
using System;

namespace CodingMilitia.EFDynamicFilteringAndSorting.Extensions
{
    public enum SortingExpressionStrategy { Hardcoded, Reflection }
    public static class Sorting
    {
        private static ISortingStrategy Helper;

        static Sorting()
        {
            Helper = new ReflectionBasedPropertyTypeInferringSortingStrategy();
        }

        public static IQueryable<TEntity> Sort<TEntity>(this IQueryable<TEntity> items, params SortCriteria[] sortCriteria)
        {
            return Helper.Sort(items, sortCriteria);
        }

        public static void SetupTestingEnvironment(SortingExpressionStrategy strategy, bool enableReflectionCaching)
        {
            switch (strategy)
            {
                case SortingExpressionStrategy.Hardcoded:
                    Helper = new HardcodedPropertyTypeInferringSortingStrategy();
                    break;
                case SortingExpressionStrategy.Reflection:
                    Helper = new ReflectionBasedPropertyTypeInferringSortingStrategy(enableReflectionCaching);
                    break;
                default:
                    throw new NotImplementedException($"The requested strategy {strategy} is not yet implemented.");
            }
        }
    }
}
