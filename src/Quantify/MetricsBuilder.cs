using System;
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
            _scheduler.Schedule(reporter, periodMilliseconds);
            return this;
        }

        public MetricsBuilder UsePercentiles(params decimal[] percentiles)
        {
            _percentiles = percentiles;
            return this;
        }

        public MetricsBuilder UseRateWindows(params int[] rateWindows)
        {
            _rateWindows = rateWindows;
            return this;
        }

        public MetricsBuilder UseHistogramSampling(ISamplingReservoirFactory factory)
        {
            _reservoirFactory = factory;
            return this;
        }

        public MetricsBuilder UseClock(IClock clock)
        {
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