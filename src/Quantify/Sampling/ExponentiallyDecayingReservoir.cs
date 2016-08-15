using System;
using System.Collections.Generic;
using System.Threading;

namespace Quantify.Sampling
{
    public class ExponentiallyDecayingReservoir<T> : IReservoir<T>
        where T : struct, IComparable
    {
        private const int DefaultSize = 1028;
        private const double DefaultAlpha = 0.015;
        private const long RescaleThreshold = 60L * 60L * 1000L * 1000L * 1000L;

        private readonly SortedList<double, WeightedSample<T>> _values;
        private SpinLock _lock;
        private readonly double _alpha;
        private readonly int _size;
        private readonly AtomicLong _count;
        private readonly AtomicLong _startTime;
        private readonly AtomicLong _nextScaleTime;
        private readonly IClock _clock;
        private readonly ThreadLocal<Random> _random = new ThreadLocal<Random>(() => new Random());

        public ExponentiallyDecayingReservoir(IClock clock)
                : this(DefaultSize, DefaultAlpha, clock)
        {
        }

        public ExponentiallyDecayingReservoir(int size, double alpha, IClock clock)
        {
            _values = new SortedList<double, WeightedSample<T>>();
            _lock = new SpinLock();
            _alpha = alpha;
            _size = size;
            _clock = clock;
            _count = new AtomicLong(0);
            _startTime = new AtomicLong(CurrentTimeInSeconds());
            _nextScaleTime = new AtomicLong(clock.CurrentTimeNanoseconds() + RescaleThreshold);
        }

        public int Size => (int)Math.Min(_size, _count.Value);


        public void Mark(T value)
        {
            Mark(value, CurrentTimeInSeconds());
        }

        public void Mark(T value, long timestamp)
        {
            RescaleIfNeeded();
            var locked = false;
            try
            {
                _lock.Enter(ref locked);
                var itemWeight = Weight(timestamp - _startTime);
                var sample = new WeightedSample<T>(value, itemWeight);
                var random = _random.Value.NextDouble();
                while (random == 0.0)
                    random = _random.Value.NextDouble();
                var priority = itemWeight / random;

                var newCount = _count.Increment();
                if (newCount <= _size)
                {
                    _values.Add(priority, sample);
                }
                else
                {
                    var first = _values.Keys[0];
                    var previousExists = _values.ContainsKey(priority);
                    _values[priority] = sample;

                    if (first < priority && !previousExists)
                    {
                        // ensure we always remove an item
                        while (!_values.Remove(first))
                        {
                            first = _values.Keys[0];
                        }
                    }
                }
            }
            finally
            {
                if (locked)
                    _lock.Exit();
            }
        }

        private void RescaleIfNeeded()
        {
            var now = _clock.CurrentTimeNanoseconds();
            var next = _nextScaleTime.Value;
            if (now >= next)
            {
                Rescale(now, next);
            }
        }

        public ISampleSet<T> GetSamples()
        {
            var locked = false;
            try
            {
                _lock.Enter(ref locked);
                return new WeightedSampleSet<T>(_count, _values.Values);
            }
            finally
            {
                if (locked)
                    _lock.Exit();
            }
        }

        private long CurrentTimeInSeconds()
        {
            const int nanosecondsInSecond = 1000000000;
            return _clock.CurrentTimeNanoseconds() / nanosecondsInSecond;
        }

        private double Weight(long t)
        {
            return Math.Exp(_alpha * t);
        }

        private void Rescale(long now, long next)
        {
            if (_nextScaleTime.CompareAndSet(next, now + RescaleThreshold))
            {
                var locked = false;
                try
                {
                    _lock.Enter(ref locked);
                    long oldStartTime = _startTime;
                    this._startTime.Set(CurrentTimeInSeconds());
                    double scalingFactor = Math.Exp(-_alpha * (_startTime - oldStartTime));

                    foreach (Double key in _values.Keys)
                    {
                        WeightedSample<T> sample = _values[key];
                        _values.Remove(key);
                        WeightedSample<T> newSample = new WeightedSample<T>(sample.Value, sample.Weight * scalingFactor);
                        _values[key * scalingFactor] = newSample;
                    }

                    // make sure the counter is in sync with the number of stored samples.
                    _count.Set(_values.Count);
                }
                finally
                {
                    if (locked)
                        _lock.Exit();
                }
            }
        }
    }

    public class ExponentiallyDecayingReservoirFactory : ISamplingReservoirFactory
    {
        public IReservoir<T> Create<T>(IClock clock) where T : struct, IComparable
        {
            return new ExponentiallyDecayingReservoir<T>(clock);
        }
    }
}