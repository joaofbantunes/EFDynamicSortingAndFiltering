using System;
using System.Linq;

namespace CodingMilitia.EFDynamicFilteringAndSortingSample.Data
{
    public static class SampleContextExtensions
    {
        public static void EnsureSeedData(this SampleContext ctx)
        {
            if (!ctx.SampleEntities.Any())
            {
                for (var i = 0; i < 20; ++i)
                {
                    ctx.SampleEntities.Add(new Model.SampleEntity
                    {
                        SomeNullableInt = i % 4 == 0 ? null : new int?(i),
                        SomeDate = DateTime.UtcNow.AddDays(i),
                        SomeNullableDate = i % 5 == 0 ? null : new DateTime?(DateTime.UtcNow.AddDays(i)),
                        SomeString = "Some String " + i,
                        SomeGuid = Guid.NewGuid(),
                        SomeNullableGuid = i % 6 == 0 ? null : new Guid?(Guid.NewGuid())
                    });
                    
                    ctx.SaveChanges();
                }
            }
        }
    }
}