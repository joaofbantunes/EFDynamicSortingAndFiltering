using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CodingMilitia.EFDynamicFilteringAndSorting.Extensions;
using CodingMilitia.EFDynamicFilteringAndSortingSample.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using CodingMilitia.EFDynamicFilteringAndSortingSample.Data.Model;

namespace CodingMilitia.EFDynamicFilteringAndSorting.Benchmark.SampleCases
{
    public class SortingBenchmark
    {
        private const int Iterations = 100;
        private SampleContext _ctx;

        [GlobalSetup(Target = nameof(BusinessAsUsualAsync))]
        public void GlobalSetupNormal()
        {
            _ctx = DbContextHelper.GetContext();
        }

        [GlobalSetup(Target = nameof(HardcodedAsync))]
        public void GlobalSetupHardcoded()
        {
            _ctx = DbContextHelper.GetContext();
            Sorting.SetupTestingEnvironment(SortingExpressionStrategy.Hardcoded, false);
        }
        [GlobalSetup(Target = nameof(ReflectionNoCacheAsync))]
        public void GlobalSetupReflectionNoCache()
        {
            _ctx = DbContextHelper.GetContext();
            Sorting.SetupTestingEnvironment(SortingExpressionStrategy.Reflection, false);
        }
        [GlobalSetup(Target = nameof(ReflectionWithCacheAsync))]
        public void GlobalSetupReflectionWithCache()
        {
            _ctx = DbContextHelper.GetContext();
            Sorting.SetupTestingEnvironment(SortingExpressionStrategy.Reflection, true);
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _ctx.Dispose();
        }

        [Benchmark]
        public async Task BusinessAsUsualAsync()
        {
            for (var i = 0; i < Iterations; ++i)
            {
                await _ctx.SampleEntities.OrderByDescending(e => e.SomeNullableInt).ToListAsync();
            }
        }

        [Benchmark]
        public async Task HardcodedAsync()
        {
            for (var i = 0; i < Iterations; ++i)
            {
                await SortUsingExtensionsAsync();
            }
        }

        [Benchmark]
        public async Task ReflectionNoCacheAsync()
        {
            for (var i = 0; i < Iterations; ++i)
            {
                await SortUsingExtensionsAsync();
            }
        }

        [Benchmark]
        public async Task ReflectionWithCacheAsync()
        {
            for (var i = 0; i < Iterations; ++i)
            {
                await SortUsingExtensionsAsync();
            }
        }

        private async Task SortUsingExtensionsAsync()
        {
            await _ctx.SampleEntities.Sort(new SortCriteria { PropertyName = nameof(SampleEntity.SomeNullableInt), Direction = SortDirection.Descending }).ToListAsync();
        }
    }
}