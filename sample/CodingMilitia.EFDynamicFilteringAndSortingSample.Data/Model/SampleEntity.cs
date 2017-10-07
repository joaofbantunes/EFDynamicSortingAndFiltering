using System;

namespace CodingMilitia.EFDynamicFilteringAndSortingSample.Data.Model
{
    public class SampleEntity
    {
        public int Id { get; set; }

        public int? SomeNullableInt { get; set; }

        public DateTime SomeDate { get; set; }

        public DateTime? SomeNullableDate { get; set; }

        public string SomeString { get; set; }

        public Guid SomeGuid { get; set; }

        public Guid? SomeNullableGuid { get; set; }
    }
}