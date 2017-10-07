using System;
using CodingMilitia.EFDynamicFilteringAndSorting.Extensions;
using CodingMilitia.EFDynamicFilteringAndSortingSample.Data;
using CodingMilitia.EFDynamicFilteringAndSortingSample.Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CodingMilitia.EFDynamicFilteringAndSortingSample.Console
{
    class Program
    {
        private static readonly string ConnectionString = "server=localhost;port=5432;user id=user;password=pass;database=EFDynamicFilteringAndSortingSample";

        static void Main(string[] args)
        {
            Sorting.SetupTestingEnvironment(SortingExpressionStrategy.Reflection, true);
            Filtering.SetupTestingEnvironment(FilteringExpressionStrategy.Reflection, true);

            var serviceProvider = new ServiceCollection()
                .AddDbContext<SampleContext>(options => options.UseNpgsql(ConnectionString))
                .BuildServiceProvider();

            using (var ctx = GetCtx(serviceProvider))
            {
                foreach (var entity in ctx.SampleEntities
                                    .Filter(new Filter { Type = FilterType.Equals, PropertyName = nameof(SampleEntity.Id), Values = new[] { "2" } })
                                    .Sort(new SortCriteria { PropertyName = nameof(SampleEntity.SomeNullableInt), Direction = SortDirection.Descending })
                                    )
                {
                    System.Console.WriteLine(GetEntityRowString(entity));
                }
            }
        }

        private static SampleContext GetCtx(ServiceProvider serviceProvider)
        {
            var ctx = serviceProvider.GetService<SampleContext>();
            ctx.Database.EnsureDeleted();
            ctx.Database.EnsureCreated();
            ctx.EnsureSeedData();

            return ctx;
        }

        private static string GetEntityRowString(SampleEntity entity)
        {
            return $"| {entity.Id} | {entity.SomeNullableInt} | {entity.SomeDate} | {entity.SomeNullableDate} | {entity.SomeString} | {entity.SomeGuid} | {entity.SomeNullableGuid} |";
        }
    }
}
