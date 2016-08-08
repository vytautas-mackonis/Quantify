namespace Quantify.Metrics
{
    public struct PercentileValue<T>
        where T: struct 
    {
        public double Quantile { get; }
        public T Percentile { get; }

        public PercentileValue(double quantile, T percentile)
        {
            Quantile = quantile;
            Percentile = percentile;
        }

        public override string ToString()
        {
            return $"{{{nameof(Quantile)}: {Quantile}, {nameof(Percentile)}: {Percentile}}}";
        }

        public bool Equals(PercentileValue<T> other)
        {
            return Quantile.Equals(other.Quantile) && Percentile.Equals(other.Percentile);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is PercentileValue<T> && Equals((PercentileValue<T>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Quantile.GetHashCode()*397) ^ Percentile.GetHashCode();
            }
        }
    }
}