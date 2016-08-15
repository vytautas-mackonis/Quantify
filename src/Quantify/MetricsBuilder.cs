using System;
using System.Linq;
using Quantify.Sampling;

namespace Quantify
{
    public class MetricsBuilder
    {
        private readonly ReportScheduler _scheduler = new ReportScheduler();
        private decimal[] _percentiles = new[] {0.5m, 0.75m, 0.95m, 0.98m, 0.99m, 0.999m};
        private int[] _rateWindows = new[] {60, 300, 900};
        private ISamplingReservoirFactory _reservoirFactory = new ExponentiallyDecayingReservoirFactory();
        private IClock _clock = new StopwatchClock();

        public MetricsBuilder ReportUsing(IMetricsReporter reporter, int periodMilliseconds)
        {
            if (reporter == null)
                throw new ArgumentNullException(nameof(reporter));

            if (periodMilliseconds <= 0)
                throw new ArgumentOutOfRangeException($"{nameof(periodMilliseconds)} must be a positive integer");

            _scheduler.Schedule(reporter, periodMilliseconds);
            return this;
        }

        public MetricsBuilder UsePercentiles(params decimal[] percentiles)
        {
            if (percentiles == null)
                throw new ArgumentNullException(nameof(percentiles));

            var invalidPercentiles = percentiles.Where(x => x <= 0 || x > 1).ToArray();
            if (invalidPercentiles.Length > 0)
                throw new ArgumentOutOfRangeException($"Invalid percentile values: [{string.Join(", ", invalidPercentiles)}]");

            _percentiles = percentiles;
            return this;
        }

        public MetricsBuilder UseRateWindows(params int[] rateWindows)
        {
            if (rateWindows == null)
                throw new ArgumentNullException(nameof(rateWindows));

            var invalidRateWindows = rateWindows.Where(x => x <= 0).ToArray();
            if (invalidRateWindows.Length > 0)
                throw new ArgumentOutOfRangeException($"Invalid rate window values: [{string.Join(", ", invalidRateWindows)}]");

            _rateWindows = rateWindows;
            return this;
        }

        public MetricsBuilder UseHistogramSampling(ISamplingReservoirFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _reservoirFactory = factory;
            return this;
        }

        public MetricsBuilder UseClock(IClock clock)
        {
            if (clock == null)
                throw new ArgumentNullException(nameof(clock));

            _clock = clock;
            return this;
        }

        public IDisposable Run()
        {
            return new MetricsEngine(_scheduler, new MetricsConfiguration(_percentiles, _rateWindows, _reservoirFactory, _clock))
                .Start();
        }
    }
}