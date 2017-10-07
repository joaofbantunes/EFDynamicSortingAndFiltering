using System;
using System.Linq.Expressions;

namespace CodingMilitia.EFDynamicFilteringAndSorting.Extensions.Factory
{
    internal static class ExpressionHelper
    {
        internal static Expression<Func<TIn, TOut>> CreatePropertyAccessor<TIn, TOut>(string propertyName)
        {
            var param = Expression.Parameter(typeof(TIn));
            var body = Expression.PropertyOrField(param, propertyName);
            return Expression.Lambda<Func<TIn, TOut>>(body, param);
        }
    }

}