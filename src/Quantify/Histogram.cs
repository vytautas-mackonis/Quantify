using System;
using System.Linq;
using System.Threading;
using Quantify.Sampling;

namespace Quantify
{
    public class Histogram<T> : IMetric
        where T : struct, IComparable
    {
        private readonly string _name;
        private readonly IReservoir<T> _reservoir;
        private readonly decimal[] _percentiles;
        private ValueHolder<T> _lastValue = new ValueHolder<T>(default(T));

        public Histogram(string name, IReservoir<T> reservoir, decimal[] percentiles)
        {
            _name = name;
            _percentiles = percentiles;
            _reservoir = reservoir;
        }

        public void Mark(T value)
        {
            Interlocked.Exchange(ref _lastValue, new ValueHolder<T>(value));
            _reservoir.Mark(value);
        }

        public void Accept(IMetricVisitor visitor)
        {
            visitor.Visit(_name, new HistogramValue<T>(_reservoir.GetSamples(), _lastValue.Value, _percentiles));
        }
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

        public HistogramValue(ISampleSet<T> sampleSet, T lastValue, decimal[] percentiles)
        {
            Count = sampleSet.Count;
            LastValue = lastValue;
            Max = sampleSet.Max;
            Min = sampleSet.Min;
            Mean = sampleSet.Mean;
            StdDev = sampleSet.StdDev;
            Percentiles = percentiles
                .Select(x => new PercentileValue<T>(x, sampleSet.GetPercentile((double)x)))
                .ToArray();
        }
    }
}
