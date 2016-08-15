using System;
using System.Collections.Generic;
using System.Linq;

namespace Quantify.Sampling
{
    internal class UniformSampleSet<T> : ISampleSet<T>
        where T : struct, IComparable
    {
        private readonly T[] _values;

        public UniformSampleSet(long count, IEnumerable<T> values)
        {
            Count = count;
            _values = values.ToArray();
            Array.Sort(_values);
        }

        public long Count { get; }

        public T GetPercentile(double quantile)
        {
            if (quantile < 0.0 || quantile > 1.0 || Double.IsNaN(quantile))
            {
                throw new ArgumentException(quantile + " is not in [0..1]");
            }

            if (_values.Length == 0)
            {
                return default(T);
            }

            var pos = quantile * (_values.Length + 1);
            var index = (int)pos;

            if (index < 1)
            {
                return _values[0];
            }

            if (index >= _values.Length)
            {
                return _values[_values.Length - 1];
            }

            //T lower = values[index - 1];
            //T upper = values[index];
            //return lower + (pos - Math.Floor(pos)) * (upper - lower);
            return _values[index];
        }

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
                foreach (var value in _values)
                {
                    sum += ToDouble(value);
                }
                return sum / _values.Length;
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

                var mean = Mean;
                var sum = 0.0;

                foreach (var value in _values)
                {
                    var diff = ToDouble(value) - mean;
                    sum += diff * diff;
                }

                var variance = sum / (_values.Length - 1);
                return Math.Sqrt(variance);
            }
        }
    }
}
