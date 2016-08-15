using System;
using System.Collections.Generic;
using System.Linq;

namespace Quantify.Sampling
{
    public class SlidingTimeWindowReservoir<T> : IReservoir<T>
        where T : struct, IComparable
    {
        private const int CollisionBuffer = 256;
        private const int TrimThreshold = 256;

        private readonly IClock _clock;
        private readonly SortedList<long, T> _measurements;
        private readonly long _window;
        private readonly AtomicLong _lastTick;
        private readonly AtomicLong _count;


        public SlidingTimeWindowReservoir(long windowNanoSeconds, IClock clock)
        {
            _clock = clock;
            _measurements = new SortedList<long, T>();
            _window = windowNanoSeconds * CollisionBuffer;
            _lastTick = new AtomicLong(clock.CurrentTimeNanoseconds() * CollisionBuffer);
            _count = new AtomicLong(0);
        }

        public int Size
        {
            get
            {
                Trim();
                return _measurements.Count;
            }
        }


        public void Mark(T value)
        {
            if (_count.Increment() % TrimThreshold == 0)
            {
                Trim();
            }
            _measurements[GetTick()] = value;
        }

        public ISampleSet<T> GetSamples()
        {
            Trim();
            return new UniformSampleSet<T>(_count, _measurements.Values);
        }

        private long GetTick()
        {
            for (;;)
            {
                var oldTick = _lastTick.Value;
                var tick = _clock.CurrentTimeNanoseconds() * CollisionBuffer;
                // ensure the tick is strictly incrementing even if there are duplicate ticks
                var newTick = tick - oldTick > 0 ? tick : oldTick + 1;
                if (_lastTick.CompareAndSet(oldTick, newTick))
                {
                    return newTick;
                }
            }
        }

        private void Trim()
        {
            foreach (var key in _measurements.Keys.TakeWhile(x => x < GetTick() - _window).ToArray())
            {
                _measurements.Remove(key);
            }
        }
    }

    public class SlidingTimeWindowReservoirFactory : ISamplingReservoirFactory
    {
        private readonly long _windowNanoSeconds;

        public SlidingTimeWindowReservoirFactory(long windowNanoSeconds)
        {
            _windowNanoSeconds = windowNanoSeconds;
        }

        public IReservoir<T> Create<T>(IClock clock) where T : struct, IComparable
        {
            return new SlidingTimeWindowReservoir<T>(_windowNanoSeconds, clock);
        }
    }
}
