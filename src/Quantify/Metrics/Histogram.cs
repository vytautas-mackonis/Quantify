using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quantify.Metrics.Sampling;

namespace Quantify.Metrics
{
    public class Histogram<T>
        where T : struct, IComparable
    {
        private readonly IReservoir<T> _reservoir;
        private readonly double[] _percentiles;
        private ValueHolder<T> _lastValue = new ValueHolder<T>(default(T));

        public Histogram(IReservoir<T> reservoir, double[] percentiles)
        {
            _percentiles = percentiles;
            _reservoir = reservoir;
        }

        public void Mark(T value)
        {
            Interlocked.Exchange(ref _lastValue, new ValueHolder<T>(value));
            _reservoir.Mark(value);
        }

        public HistogramValue<T> Value { get { return new HistogramValue<T>(_reservoir.GetSamples(), _lastValue.Value, _percentiles); } }
    }

    public class HistogramValue<T>
        where T: struct, IComparable
    {
        public long Count { get; }
        public T LastValue { get; }
        public T Max { get; }
        public T Min { get; }
        public double Mean { get; }
        public double StdDev { get; }
        public PercentileValue<T>[] Percentiles { get; }

        public HistogramValue(ISampleSet<T> sampleSet, T lastValue, double[] percentiles)
        {
            Count = sampleSet.Count;
            LastValue = lastValue;
            Max = sampleSet.Max;
            Min = sampleSet.Min;
            Mean = sampleSet.Mean;
            StdDev = sampleSet.StdDev;
            Percentiles = percentiles
                .Select(x => new PercentileValue<T>(x, sampleSet.GetPercentile(x)))
                .ToArray();
        }
    }
}
