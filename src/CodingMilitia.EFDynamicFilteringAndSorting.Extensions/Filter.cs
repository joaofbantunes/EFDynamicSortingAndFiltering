using System.Collections.Generic;

namespace CodingMilitia.EFDynamicFilteringAndSorting.Extensions
{
    public enum FilterType { Equals, NotEquals, Range, Contains, Less, LessOrEqual, Greater, GreaterOrEqual }

    public class Filter
    {
        public string PropertyName { get; set; }
        public IEnumerable<string> Values { get; set; }
        public FilterType Type { get; set; }
    }
}
