using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Quantify.Metrics.Sampling;
using Quantify.Metrics.Time;

namespace Quantify.Metrics
{
    public class Meter
    {
        private const int TickSeconds = 5;
        private const long NanosecondsInSecond = 1000000000L;
        private const long TickInterval = TickSeconds * NanosecondsInSecond;

        private readonly AtomicLong count = new AtomicLong(0);
        private readonly long startTime;
        private readonly AtomicLong lastTick;
        private readonly IClock clock;

        private readonly IDictionary<int, ExponentiallyWeightedMovingAverage> _movingAverages;

        public Meter(IClock clock)
        {
            this.clock = clock;
            this.startTime = this.clock.CurrentTimeNanoseconds();
            this.lastTick = new AtomicLong(startTime);

            _movingAverages = new Dictionary<int, ExponentiallyWeightedMovingAverage>
            {
                { 60, ExponentiallyWeightedMovingAverage.oneMinuteEWMA() },
                { 300, ExponentiallyWeightedMovingAverage.fiveMinuteEWMA() },
                { 900, ExponentiallyWeightedMovingAverage.fifteenMinuteEWMA() }
            };
        }

        public void Mark(long count = 1l)
        {
            tickIfNecessary();
            this.count.Add(count);
            foreach (var rate in _movingAverages.Values)
            {
                rate.update(count);
            }
        }

        private double getMeanRate()
        {
            var value = count.Value;
            if (value == 0)
            {
                return 0.0;
            }
            else
            {
                double elapsed = (clock.CurrentTimeNanoseconds() - startTime);
                return value / elapsed * NanosecondsInSecond;
            }
        }

        public MeterValue Value
        {
            get
            {
                var rates = _movingAverages.Select(x => new RateValue(x.Key, x.Value.getRate()*NanosecondsInSecond))
                    .ToArray();
                return new MeterValue(count.Value, getMeanRate(), rates);
            }
        }

        private void tickIfNecessary()
        {
            
            long oldTick = lastTick.Value;
            long newTick = clock.CurrentTimeNanoseconds();
            long age = newTick - oldTick;

            if (age > TickInterval)
            {
                long newIntervalStartTick = newTick - age % TickInterval;
                if (lastTick.CompareAndSet(oldTick, newIntervalStartTick))
                {
                    long requiredTicks = age / TickInterval;
                    for (long i = 0; i < requiredTicks; i++)
                    {
                        foreach (var rate in _movingAverages.Values)
                        {
                            rate.tick();
                        }
                    }
                }
            }
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
        public int IntervalSeconds { get; }
        public double Rate { get; }

        public RateValue(int intervalSeconds, double rate)
        {
            IntervalSeconds = intervalSeconds;
            Rate = rate;
        }

        public bool Equals(RateValue other)
        {
            return IntervalSeconds == other.IntervalSeconds && Rate.Equals(other.Rate);
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
                return (IntervalSeconds*397) ^ Rate.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"{{{nameof(IntervalSeconds)}: {IntervalSeconds}, {nameof(Rate)}: {Rate}}}";
        }
    }
}
