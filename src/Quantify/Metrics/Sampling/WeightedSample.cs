using System;

namespace Quantify.Metrics.Sampling
{
    public class WeightedSample<T> : IComparable<WeightedSample<T>>
        where T: struct, IComparable
    {
        public T Value { get; }
        public double Weight { get; }

        public WeightedSample(T value, double weight)
        {
            this.Value = value;
            this.Weight = weight;
        }

        public int CompareTo(WeightedSample<T> other)
        {
            return Value.CompareTo(other.Value);
        }
    }
}