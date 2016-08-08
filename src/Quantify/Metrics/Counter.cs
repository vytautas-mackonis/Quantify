using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Quantify.Metrics
{
    public class Counter
    {
        private long _value = 0;

        public void Increment()
        {
            Interlocked.Increment(ref _value);
        }

        public void Increment(long value)
        {
            Interlocked.Add(ref _value, value);
        }

        public void Decrement()
        {
            Interlocked.Decrement(ref _value);
        }

        public void Decrement(long value)
        {
            Interlocked.Add(ref _value, -value);
        }

        public CounterValue Value => new CounterValue(Volatile.Read(ref _value));
    }

    public class CounterValue
    {
        public long Count { get; }

        public CounterValue(long count)
        {
            Count = count;
        }
    }
}
