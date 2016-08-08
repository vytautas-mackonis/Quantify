using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quantify.Metrics.Sampling
{
    public class SlidingWindowReservoir<T> : IReservoir<T>
        where T: struct, IComparable
    {
        private readonly T[] _measurements;
        private readonly AtomicLong _count = new AtomicLong(0L);
        private readonly object _lock = new object();

        public SlidingWindowReservoir(int size)
        {
            _measurements = new T[size];
        }

        public int Size
        {
            get
            {
                lock (_lock)
                {
                    return (int)Math.Min(_count, _measurements.Length);
                }
            }
        }

        public void Mark(T value)
        {
            lock (_lock)
            {
                _measurements[(int)(_count % _measurements.Length)] = value;
                _count.Increment();
            }
        }

        public ISampleSet<T> GetSamples()
        {
            var values = new T[Size];
            for (var i = 0; i < values.Length; i++)
            {
                lock(_lock) {
                    values[i] = _measurements[i];
                }
            }
            return new UniformSampleSet<T>(_count.Value, values);
        }
    }
}
