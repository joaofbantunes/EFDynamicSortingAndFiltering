using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using CodingMilitia.EFDynamicFilteringAndSorting.Benchmark.SampleCases;

namespace CodingMilitia.EFDynamicFilteringAndSorting.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var samples = new Type[]
            {
                typeof(FilteringBenchmark),
                typeof(SortingBenchmark)
            };

            var summary = BenchmarkRunner.Run(samples[int.Parse(args[0])],
                    ManualConfig.Create(DefaultConfig.Instance).With(MemoryDiagnoser.Default)
                );
        }
    }
}
