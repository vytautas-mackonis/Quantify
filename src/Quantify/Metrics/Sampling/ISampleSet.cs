using System;

namespace Quantify.Metrics.Sampling
{
    public interface ISampleSet<T>
        where T: struct, IComparable
    {
        T GetPercentile(double quantile);
        long Count { get; }
        T Max { get; }
        T Min { get; }
        double Mean { get; }
        double StdDev { get; }
    }
}