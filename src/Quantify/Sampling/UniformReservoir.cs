using System;
using System.Threading;

namespace Quantify.Sampling
{
    public class UniformReservoir<T> : IReservoir<T>
        where T : struct, IComparable
    {
        private const int DefaultSize = 1028;
        private const int BitsPerLong = 63;
        private readonly AtomicLong _count = new AtomicLong(0L);
        private readonly ValueHolder<T>[] _values;
        private static readonly ThreadLocal<Random> Random = new ThreadLocal<Random>(() => new Random());

        public UniformReservoir()
            : this(DefaultSize)
        {
        }

        public UniformReservoir(int size)
        {
            _values = new ValueHolder<T>[size];
            for (var i = 0; i < _values.Length; i++)
            {
                _values[i] = new ValueHolder<T>(default(T));
            }
            _count.Set(0);
        }

        public int Size
        {
            get
            {
                var c = _count.Value;
                if (c > _values.Length)
                {
                    return _values.Length;
                }
                return (int)c;
            }
        }

        public void Mark(T value)
        {
            var c = _count.Increment();
            if (c <= _values.Length)
            {
                _values[(int)c - 1] = new ValueHolder<T>(value);
            }
            else
            {
                var r = NextLong(c);
                if (r < _values.Length)
                {
                    _values[(int)r] = new ValueHolder<T>(value);
                }
            }
        }

        private static long NextLong(long n)
        {
            long bits, val;
            do
            {
                var randomBytes = new byte[8];
                Random.Value.NextBytes(randomBytes);
                var randomLong = BitConverter.ToInt64(randomBytes, 0);

                bits = randomLong & (~(1L << BitsPerLong));
                val = bits % n;
            } while (bits - val + (n - 1) < 0L);
            return val;
        }

        public ISampleSet<T> GetSamples()
        {
            var s = Size;
            var copy = new T[s];
            for (var i = 0; i < s; i++)
            {
                copy[i] = _values[i].Value;
            }
            return new UniformSampleSet<T>(_count, copy);
        }
    }

    public class UniformReservoirFactory : ISamplingReservoirFactory
    {
        public IReservoir<T> Create<T>(IClock clock) where T : struct, IComparable
        {
            return new UniformReservoir<T>();
        }
    }
}
