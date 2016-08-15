﻿using System.Threading;

namespace Quantify
{
    public class Counter : IMetric
    {
        private readonly string _name;
        private long _value = 0;

        public Counter(string name)
        {
            _name = name;
        }

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

        public void Accept(IMetricVisitor visitor)
        {
            visitor.Visit(_name, new CounterValue(Volatile.Read(ref _value)));
        }
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