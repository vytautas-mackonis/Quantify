using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Quantify.Sampling;

namespace Quantify
{
    public class Meter : IMetric
    {
        private const int TickSeconds = 5;
        private const long NanosecondsInSecond = 1000000000L;
        private const long TickInterval = TickSeconds * NanosecondsInSecond;

        private readonly string _name;
        private readonly AtomicLong _count = new AtomicLong(0);
        private readonly long _startTime;
        private readonly AtomicLong _lastTick;
        private readonly IClock _clock;

        private readonly IDictionary<int, ExponentiallyWeightedMovingAverage> _movingAverages;

        public Meter(string name, IClock clock, int[] movingRateWindowSeconds)
        {
            _name = name;
            _clock = clock;
            _startTime = _clock.CurrentTimeNanoseconds();
            _lastTick = new AtomicLong(_startTime);

            _movingAverages = new ReadOnlyDictionary<int, ExponentiallyWeightedMovingAverage>(
                movingRateWindowSeconds.ToDictionary(x => x, x => new ExponentiallyWeightedMovingAverage(x))
            );
        }

        public void Mark(long count = 1L)
        {
            TickIfNecessary();
            _count.Add(count);
            foreach (var rate in _movingAverages.Values)
            {
                rate.Update(count);
            }
        }

        private double GetMeanRate()
        {
            var value = _count.Value;
            if (value == 0)
            {
                return 0.0;
            }
            else
            {
                double elapsed = (_clock.CurrentTimeNanoseconds() - _startTime);
                return value / elapsed * NanosecondsInSecond;
            }
        }

        private void TickIfNecessary()
        {
            
            var oldTick = _lastTick.Value;
            var newTick = _clock.CurrentTimeNanoseconds();
            var age = newTick - oldTick;

            if (age > TickInterval)
            {
                var newIntervalStartTick = newTick - age % TickInterval;
                if (_lastTick.CompareAndSet(oldTick, newIntervalStartTick))
                {
                    var requiredTicks = age / TickInterval;
                    for (long i = 0; i < requiredTicks; i++)
                    {
                        foreach (var rate in _movingAverages.Values)
                        {
                            rate.Tick();
                        }
                    }
                }
            }
        }

        public void Accept(IMetricVisitor visitor)
        {
            var rates = _movingAverages.Select(x => new RateValue(x.Key, x.Value.GetRate() * NanosecondsInSecond))
                .ToArray();
            visitor.Visit(_name, new MeterValue(_count.Value, GetMeanRate(), rates));
        }
    }

    public class MeterValue
    {
        public long Count { get; }
        public double MeanRate { get; }
        public RateValue[] MovingRates { get; }

        public MeterValue(long count, double meanRate, RateValue[] movingRates)
        {
            Count = count;
            MeanRate = meanRate;
            MovingRates = movingRates;
        }
    }

    public struct RateValue
    {
        public int WindowSeconds { get; }
        public double Rate { get; }

        public RateValue(int windowSeconds, double rate)
        {
            WindowSeconds = windowSeconds;
            Rate = rate;
        }

        public bool Equals(RateValue other)
        {
            return WindowSeconds == other.WindowSeconds && Rate.Equals(other.Rate);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is RateValue && Equals((RateValue) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (WindowSeconds*397) ^ Rate.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"{{{nameof(WindowSeconds)}: {WindowSeconds}, {nameof(Rate)}: {Rate}}}";
        }
    }
}
