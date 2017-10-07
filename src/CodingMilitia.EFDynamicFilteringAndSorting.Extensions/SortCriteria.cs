using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingMilitia.EFDynamicFilteringAndSorting.Extensions
{
    public enum SortDirection { Ascending, Descending }

    public class SortCriteria
    {
        public string PropertyName { get; set; }
        public SortDirection Direction { get; set; }
    }
}
