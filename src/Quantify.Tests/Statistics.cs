using System;
using System.Collections.Generic;
using System.Linq;

namespace Quantify.Tests
{
    public static class Statistics
    {
        public static double PopulationStandardDeviation(IEnumerable<double> numbers)
        {
            var numberSet = numbers.ToList();
            double mean = numberSet.Average();

            return Math.Sqrt(numberSet.Sum(x => Math.Pow(x - mean, 2)) / numberSet.Count);
        }

        public static double SampleStandardDeviation(IEnumerable<double> numbers)
        {
            var numberSet = numbers.ToList();
            double mean = numberSet.Sum() / numberSet.Count;

            return Math.Sqrt(numberSet.Sum(x => Math.Pow(x - mean, 2)) / (numberSet.Count - 1));
        }

        public static T Percentile<T>(IEnumerable<T> tests, double quantile)
            where T: IComparable
        {
            var ordered = tests.OrderBy(x => x).ToArray();
            var take = (int)Math.Floor(ordered.Length * quantile) + 1;
            return ordered.Take(take).LastOrDefault();
        }
    }
}