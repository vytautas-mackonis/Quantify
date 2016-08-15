using System;
using System.Collections.Generic;
using System.Linq;

namespace Quantify.Sampling
{
    internal class WeightedSampleSet<T> : ISampleSet<T>
        where T : struct, IComparable
    {
        private readonly T[] _values;
        private readonly double[] _normWeights;
        private readonly double[] _quantiles;

        public WeightedSampleSet(long count, IList<WeightedSample<T>> values)
        {
            Count = count;
            WeightedSample<T>[] copy = values.ToArray();
            Array.Sort(copy);


            _values = new T[copy.Length];
            _normWeights = new double[copy.Length];
            _quantiles = new double[copy.Length];


            double sumWeight = 0;
            foreach (WeightedSample<T> sample in copy)
            {
                sumWeight += sample.Weight;
            }

            for (var i = 0; i < copy.Length; i++)
            {
                _values[i] = copy[i].Value;
                _normWeights[i] = copy[i].Weight/sumWeight;
            }

            for (var i = 1; i < copy.Length; i++)
            {
                _quantiles[i] = _quantiles[i - 1] + _normWeights[i - 1];
            }
        }

        public T GetPercentile(double quantile)
        {
            if (quantile < 0.0 || quantile > 1.0 || double.IsNaN(quantile))
            {
                throw new ArgumentException(quantile + " is not in [0..1]");
            }

            if (_values.Length == 0)
            {
                return default(T);
            }

            var posx = Array.BinarySearch(_quantiles, quantile);
            if (posx < 0)
                posx = ((-posx) - 1) - 1;

            if (posx < 1)
            {
                return _values[0];
            }

            if (posx >= _values.Length)
            {
                return _values[_values.Length - 1];
            }

            return _values[posx];
        }

        public long Count { get; }

        public T Max
        {
            get
            {
                if (_values.Length == 0)
                {
                    return default(T);
                }
                return _values[_values.Length - 1];
            }
        }

        public T Min
        {
            get
            {
                if (_values.Length == 0)
                {
                    return default(T);
                }
                return _values[0];
            }
        }

        public double Mean
        {
            get
            {
                if (_values.Length == 0)
                {
                    return 0;
                }

                double sum = 0;
                for (var i = 0; i < _values.Length; i++)
                {
                    sum += ToDouble(_values[i]) * _normWeights[i];
                }
                return sum;
            }
        }

        private static double ToDouble(T value)
        {
            return (double)Convert.ChangeType(value, typeof(double));
        }

        public double StdDev
        {
            get
            {
                // two-pass algorithm for variance, avoids numeric overflow

                if (_values.Length <= 1)
                {
                    return 0;
                }

                double variance = 0;
                var mean = Mean;

                for (int i = 0; i < _values.Length; i++)
                {
                    var diff = ToDouble(_values[i]) - mean;
                    variance += _normWeights[i] * diff * diff;
                }

                return Math.Sqrt(variance);
            }
        }
}
}
