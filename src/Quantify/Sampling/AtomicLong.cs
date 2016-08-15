using System.Threading;

namespace Quantify.Sampling
{
    internal class AtomicLong
    {
        private long _value;

        public AtomicLong(long value)
        {
            _value = value;
        }

        public long Value => Volatile.Read(ref _value);

        public static implicit operator long(AtomicLong value)
        {
            return value.Value;
        }

        public long Increment()
        {
            return Interlocked.Increment(ref _value);
        }

        public bool CompareAndSet(long expectedValue, long update)
        {
            return Interlocked.CompareExchange(ref _value, update, expectedValue) == expectedValue;
        }

        public void Set(long value)
        {
            Interlocked.Exchange(ref _value, value);
        }

        public void Add(long value)
        {
            Interlocked.Add(ref _value, value);
        }

        public long GetAndReset()
        {
            return Interlocked.Exchange(ref _value, 0L);
        }
    }
}