using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quantify.Sampling;

namespace Quantify
{
    public class Histogram<T> : IMetric
        where T : struct, IComparable, IConvertible
    {
        private readonly string _name;
        private readonly IReservoir<T> _reservoir;
        private readonly decimal[] _percentiles;
        private ValueHolder<T> _lastValue = new ValueHolder<T>(default(T));

        public Histogram(string name, IReservoir<T> reservoir, decimal[] percentiles)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"{nameof(name)} must be non-empty.");

            if (reservoir == null)
                throw new ArgumentNullException(nameof(reservoir));

            if (percentiles == null)
                throw new ArgumentNullException(nameof(percentiles));

            _name = name;
            _percentiles = percentiles;
            _reservoir = reservoir;
        }

        public void Mark(T value)
        {
            Interlocked.Exchange(ref _lastValue, new ValueHolder<T>(value));
            _reservoir.Mark(value);
        }

        internal HistogramValue<T> Value => new HistogramValue<T>(_reservoir.GetSamples(), _lastValue.Value, _percentiles);

        public async Task AcceptAsync(IMetricVisitor visitor)
        {
            await visitor.VisitAsync(_name, Value);
        }
    }

    public class HistogramValue<T>
        where T: struct, IComparable, IConvertible
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
