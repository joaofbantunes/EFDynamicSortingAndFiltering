using CodingMilitia.EFDynamicFilteringAndSortingSample.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CodingMilitia.EFDynamicFilteringAndSorting.Benchmark
{
    public static class DbContextHelper
    {
        private static readonly string ConnectionString = "server=localhost;port=5432;user id=user;password=pass;database=EFDynamicFilteringAndSortingSample";
        public static SampleContext GetContext()
        {
            var serviceProvider = new ServiceCollection()
                            .AddDbContext<SampleContext>(options => options.UseNpgsql(ConnectionString))
                            .BuildServiceProvider();

            var ctx = serviceProvider.GetService<SampleContext>();
            ctx.Database.EnsureDeleted();
            ctx.Database.EnsureCreated();
            ctx.EnsureSeedData();
            return ctx;
        }
    }
}