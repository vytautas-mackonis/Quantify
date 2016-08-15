using System;
using System.Linq;
using Moq;
using Xunit;
using Quantify;
using Quantify.Sampling;

namespace Quantify.Tests
{
    public class MetricsRegistryTests
    {
        private static readonly decimal[] DefaultQuantiles = new[] { 0.5m, 0.75m, 0.95m, 0.98m, 0.99m, 0.999m };
        private static readonly int[] DefaultRateWindows = new[] {60, 300, 900};

        [Fact]
        public void CreatingCounterThrowsWhenNotInitialized()
        {
            Assert.Throws<MetricsConfigurationException>(() => MetricsRegistry.Counter("foo"));
        }

        [Fact]
        public void CreatingCounterThrowsAfterDisposed()
        {
            using (Metrics.Configure().Run())
            {
            }

            Assert.Throws<MetricsConfigurationException>(() => MetricsRegistry.Counter("foo"));
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        public void CreatesCounter(string name)
        {
            using (Metrics.Configure().Run())
            {
                var counter = MetricsRegistry.Counter(name);
                Assert.Contains(counter, MetricsRegistry.ListMetrics());
                Assert.Equal(name, counter.Name());
                Assert.Equal(0, counter.Value().Count);
            }
        }

        [Fact]
        public void CreatingIntGaugeThrowsWhenNotInitialized()
        {
            Assert.Throws<MetricsConfigurationException>(() => MetricsRegistry.Gauge("foo", () => 1));
        }

        [Fact]
        public void CreatingIntGaugeThrowsAfterDisposed()
        {
            using (Metrics.Configure().Run())
            {
            }

            Assert.Throws<MetricsConfigurationException>(() => MetricsRegistry.Gauge("foo", () => 1));
        }

        [Theory]
        [InlineData("foo", 1)]
        [InlineData("bar", 3)]
        public void CreatesIntGauge(string name, int value)
        {
            using (Metrics.Configure().Run())
            {
                var metric = MetricsRegistry.Gauge(name, () => value);
                Assert.Contains(metric, MetricsRegistry.ListMetrics());
                Assert.Equal(name, metric.Name());
                Assert.Equal(value, metric.Value().Value);
            }
        }

        [Fact]
        public void CreatingLongGaugeThrowsWhenNotInitialized()
        {
            Assert.Throws<MetricsConfigurationException>(() => MetricsRegistry.Gauge("foo", () => 1L));
        }

        [Fact]
        public void CreatingLongGaugeThrowsAfterDisposed()
        {
            using (Metrics.Configure().Run())
            {
            }

            Assert.Throws<MetricsConfigurationException>(() => MetricsRegistry.Gauge("foo", () => 1L));
        }

        [Theory]
        [InlineData("foo", 1L)]
        [InlineData("bar", 3L)]
        public void CreatesLongGauge(string name, long value)
        {
            using (Metrics.Configure().Run())
            {
                var metric = MetricsRegistry.Gauge(name, () => value);
                Assert.Contains(metric, MetricsRegistry.ListMetrics());
                Assert.Equal(name, metric.Name());
                Assert.Equal(value, metric.Value().Value);
            }
        }

        [Fact]
        public void CreatingDoubleGaugeThrowsWhenNotInitialized()
        {
            Assert.Throws<MetricsConfigurationException>(() => MetricsRegistry.Gauge("foo", () => 1.0));
        }

        [Fact]
        public void CreatingDoubleGaugeThrowsAfterDisposed()
        {
            using (Metrics.Configure().Run())
            {
            }

            Assert.Throws<MetricsConfigurationException>(() => MetricsRegistry.Gauge("foo", () => 1.0));
        }

        [Theory]
        [InlineData("foo", 0.7)]
        [InlineData("bar", 14.45)]
        public void CreatesDoubleGauge(string name, double value)
        {
            using (Metrics.Configure().Run())
            {
                var metric = MetricsRegistry.Gauge(name, () => value);
                Assert.Contains(metric, MetricsRegistry.ListMetrics());
                Assert.Equal(name, metric.Name());
                Assert.Equal(value, metric.Value().Value);
            }
        }

        [Fact]
        public void CreatingDecimalGaugeThrowsWhenNotInitialized()
        {
            Assert.Throws<MetricsConfigurationException>(() => MetricsRegistry.Gauge("foo", () => 1.0m));
        }

        [Fact]
        public void CreatingDecimalGaugeThrowsAfterDisposed()
        {
            using (Metrics.Configure().Run())
            {
            }

            Assert.Throws<MetricsConfigurationException>(() => MetricsRegistry.Gauge("foo", () => 1.0m));
        }

        [Theory]
        [InlineData("foo", "0.7")]
        [InlineData("bar", "14.45")]
        public void CreatesDecimalGauge(string name, string value)
        {
            using (Metrics.Configure().Run())
            {
                var decimalValue = decimal.Parse(value);
                var metric = MetricsRegistry.Gauge(name, () => decimalValue);
                Assert.Contains(metric, MetricsRegistry.ListMetrics());
                Assert.Equal(name, metric.Name());
                Assert.Equal(decimalValue, metric.Value().Value);
            }
        }

        [Fact]
        public void CreatingLongHistogramThrowsWhenNotInitialized()
        {
            Assert.Throws<MetricsConfigurationException>(() => MetricsRegistry.LongHistogram("foo"));
        }

        [Fact]
        public void CreatingLongHistogramThrowsAfterDisposed()
        {
            using (Metrics.Configure().Run())
            {
            }

            Assert.Throws<MetricsConfigurationException>(() => MetricsRegistry.LongHistogram("foo"));
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        public void CreatesLongHistogram(string name)
        {
            using (Metrics.Configure().Run())
            {
                var metric = MetricsRegistry.LongHistogram(name);
                Assert.Contains(metric, MetricsRegistry.ListMetrics());
                Assert.Equal(name, metric.Name());

                var value = metric.Value();
                Assert.Equal(0, value.Count);
                Assert.Equal(DefaultQuantiles.Length, value.Percentiles.Length);
                Assert.Equal(DefaultQuantiles, value.Percentiles.Select(x => x.Quantile));
                Assert.Equal(Enumerable.Repeat(0L, DefaultQuantiles.Length), value.Percentiles.Select(x => x.Percentile));
            }
        }

        [Theory]
        [InlineData(new object[] { new[] { "0.2", "0.3", "0.5" } })]
        [InlineData(new object[] { new[] { "0.8", "0.999" } })]
        public void CreatesLongHistogramWithCustomPercentiles(string[] quantileStrings)
        {
            var quantiles = quantileStrings.Select(decimal.Parse).ToArray();

            using (Metrics.Configure().UsePercentiles(quantiles).Run())
            {
                var metric = MetricsRegistry.LongHistogram("foo");

                var value = metric.Value();
                Assert.Equal(0, value.Count);
                Assert.Equal(quantiles.Length, value.Percentiles.Length);
                Assert.Equal(quantiles, value.Percentiles.Select(x => x.Quantile));
                Assert.Equal(Enumerable.Repeat(0L, quantiles.Length), value.Percentiles.Select(x => x.Percentile));
            }
        }

        [Fact]
        public void CreatesLongHistogramWithCustomSamplingReservoir()
        {
            var reservoirFactory = Mock.Of<ISamplingReservoirFactory>();
            var reservoir = Mock.Of<IReservoir<long>>();

            Func<double, long> percentileTransform = q => (long) ((q + 1)*100);
            var sampleSet = new FakeSampleSet<long>(percentileTransform, 1, 2, 3, 4, 5);
            var expectedPercentiles = DefaultQuantiles.Select(q => percentileTransform((double)q));

            Mock.Get(reservoirFactory).Setup(x => x.Create<long>(It.IsAny<IClock>()))
                .Returns(reservoir);
            Mock.Get(reservoir).Setup(x => x.GetSamples())
                .Returns(sampleSet);

            using (Metrics.Configure().UseHistogramSampling(reservoirFactory).Run())
            {
                var metric = MetricsRegistry.LongHistogram("foo");

                var value = metric.Value();
                Assert.Equal(DefaultQuantiles.Length, value.Percentiles.Length);
                Assert.Equal(1, value.Count);
                Assert.Equal(2, value.Max);
                Assert.Equal(3, value.Min);
                Assert.Equal(4, value.Mean);
                Assert.Equal(5, value.StdDev);
                Assert.Equal(expectedPercentiles, value.Percentiles.Select(x => x.Percentile));
            }
        }

        [Fact]
        public void CreatesLongHistogramWithCustomClock()
        {
            var clock = Mock.Of<IClock>();
            Mock.Get(clock).Setup(x => x.CurrentTimeNanoseconds())
                .Returns(() => DateTime.UtcNow.Ticks * 100);

            using (Metrics.Configure().UseClock(clock).Run())
            {
                MetricsRegistry.LongHistogram("foo");
                Mock.Get(clock).VerifyAll();
            }
        }

        [Fact]
        public void CreatingDoubleHistogramThrowsWhenNotInitialized()
        {
            Assert.Throws<MetricsConfigurationException>(() => MetricsRegistry.DoubleHistogram("foo"));
        }

        [Fact]
        public void CreatingDoubleHistogramThrowsAfterDisposed()
        {
            using (Metrics.Configure().Run())
            {
            }

            Assert.Throws<MetricsConfigurationException>(() => MetricsRegistry.DoubleHistogram("foo"));
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        public void CreatesDoubleHistogram(string name)
        {
            using (Metrics.Configure().Run())
            {
                var metric = MetricsRegistry.DoubleHistogram(name);
                Assert.Contains(metric, MetricsRegistry.ListMetrics());
                Assert.Equal(name, metric.Name());

                var value = metric.Value();
                Assert.Equal(0, value.Count);
                Assert.Equal(DefaultQuantiles.Length, value.Percentiles.Length);
                Assert.Equal(DefaultQuantiles, value.Percentiles.Select(x => x.Quantile));
                Assert.Equal(Enumerable.Repeat(0.0, DefaultQuantiles.Length), value.Percentiles.Select(x => x.Percentile));
            }
        }

        [Fact]
        public void CreatesDoubleHistogramWithCustomSamplingReservoir()
        {
            var reservoirFactory = Mock.Of<ISamplingReservoirFactory>();
            var reservoir = Mock.Of<IReservoir<double>>();

            Func<double, double> percentileTransform = q => q;
            var sampleSet = new FakeSampleSet<double>(percentileTransform, 1, 2, 3, 4, 5);
            var expectedPercentiles = DefaultQuantiles.Select(q => percentileTransform((double)q));

            Mock.Get(reservoirFactory).Setup(x => x.Create<double>(It.IsAny<IClock>()))
                .Returns(reservoir);
            Mock.Get(reservoir).Setup(x => x.GetSamples())
                .Returns(sampleSet);

            using (Metrics.Configure().UseHistogramSampling(reservoirFactory).Run())
            {
                var metric = MetricsRegistry.DoubleHistogram("foo");

                var value = metric.Value();
                Assert.Equal(DefaultQuantiles.Length, value.Percentiles.Length);
                Assert.Equal(1, value.Count);
                Assert.Equal(2, value.Max);
                Assert.Equal(3, value.Min);
                Assert.Equal(4, value.Mean);
                Assert.Equal(5, value.StdDev);
                Assert.Equal(expectedPercentiles, value.Percentiles.Select(x => x.Percentile));
            }
        }

        [Theory]
        [InlineData(new object[] { new[] { "0.2", "0.3", "0.5" } })]
        [InlineData(new object[] { new[] { "0.8", "0.999" } })]
        public void CreatesDoubleHistogramWithCustomPercentiles(string[] quantileStrings)
        {
            var quantiles = quantileStrings.Select(decimal.Parse).ToArray();

            using (Metrics.Configure().UsePercentiles(quantiles).Run())
            {
                var metric = MetricsRegistry.DoubleHistogram("foo");

                var value = metric.Value();
                Assert.Equal(0, value.Count);
                Assert.Equal(quantiles.Length, value.Percentiles.Length);
                Assert.Equal(quantiles, value.Percentiles.Select(x => x.Quantile));
                Assert.Equal(Enumerable.Repeat(0.0, quantiles.Length), value.Percentiles.Select(x => x.Percentile));
            }
        }

        [Fact]
        public void CreatesDoubleHistogramWithCustomClock()
        {
            var clock = Mock.Of<IClock>();
            Mock.Get(clock).Setup(x => x.CurrentTimeNanoseconds())
                .Returns(() => DateTime.UtcNow.Ticks * 100);

            using (Metrics.Configure().UseClock(clock).Run())
            {
                MetricsRegistry.DoubleHistogram("foo");
                Mock.Get(clock).VerifyAll();
            }
        }

        [Fact]
        public void CreatingMeterThrowsWhenNotInitialized()
        {
            Assert.Throws<MetricsConfigurationException>(() => MetricsRegistry.Meter("foo"));
        }

        [Fact]
        public void CreatingMeterhrowsAfterDisposed()
        {
            using (Metrics.Configure().Run())
            {
            }

            Assert.Throws<MetricsConfigurationException>(() => MetricsRegistry.Meter("foo"));
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        public void CreatesMeterDefaultRateWindows(string name)
        {
            using (Metrics.Configure().Run())
            {
                var metric = MetricsRegistry.Meter(name);
                Assert.Contains(metric, MetricsRegistry.ListMetrics());
                Assert.Equal(name, metric.Name());

                var value = metric.Value();
                Assert.Equal(0, value.Count);

                Assert.Equal(DefaultRateWindows.Length, value.MovingRates.Length);
                Assert.Equal(DefaultRateWindows, value.MovingRates.Select(x => x.WindowSeconds));
                Assert.Equal(Enumerable.Repeat(0.0, DefaultRateWindows.Length), value.MovingRates.Select(x => x.Rate));
            }
        }

        [Theory]
        [InlineData(new [] { 1, 2 })]
        [InlineData(new[] { 50, 75, 100 })]
        public void CreatesMeterCustomRateWindows(int[] rateWindows)
        {
            using (Metrics.Configure().UseRateWindows(rateWindows).Run())
            {
                var metric = MetricsRegistry.Meter("foo");

                var value = metric.Value();

                Assert.Equal(rateWindows.Length, value.MovingRates.Length);
                Assert.Equal(rateWindows, value.MovingRates.Select(x => x.WindowSeconds));
                Assert.Equal(Enumerable.Repeat(0.0, rateWindows.Length), value.MovingRates.Select(x => x.Rate));
            }
        }

        [Fact]
        public void CreatesMeterCustomClock()
        {
            var clock = Mock.Of<IClock>();
            Mock.Get(clock).Setup(x => x.CurrentTimeNanoseconds())
                .Returns(() => DateTime.UtcNow.Ticks*100);

            using (Metrics.Configure().UseClock(clock).Run())
            {
                MetricsRegistry.Meter("foo");
                Mock.Get(clock).VerifyAll();
            }
        }

        [Fact]
        public void CreatingTimerThrowsWhenNotInitialized()
        {
            Assert.Throws<MetricsConfigurationException>(() => MetricsRegistry.Timer("foo"));
        }

        [Fact]
        public void CreatingTimerThrowsAfterDisposed()
        {
            using (Metrics.Configure().Run())
            {
            }

            Assert.Throws<MetricsConfigurationException>(() => MetricsRegistry.Timer("foo"));
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        public void CreatesTimer(string name)
        {
            using (Metrics.Configure().Run())
            {
                var metric = MetricsRegistry.Timer(name);
                Assert.Contains(metric, MetricsRegistry.ListMetrics());
                Assert.Equal(name, metric.Name());

                var value = metric.Value();
                Assert.Equal(0, value.Latencies.Count);
                Assert.Equal(DefaultQuantiles.Length, value.Latencies.Percentiles.Length);
                Assert.Equal(DefaultQuantiles, value.Latencies.Percentiles.Select(x => x.Quantile));
                Assert.Equal(Enumerable.Repeat(0L, DefaultQuantiles.Length), value.Latencies.Percentiles.Select(x => x.Percentile));

                var rate = metric.Value().Rate;

                Assert.Equal(DefaultRateWindows.Length, rate.MovingRates.Length);
                Assert.Equal(DefaultRateWindows, rate.MovingRates.Select(x => x.WindowSeconds));
                Assert.Equal(Enumerable.Repeat(0.0, DefaultRateWindows.Length), rate.MovingRates.Select(x => x.Rate));

                rate = metric.Value().ErrorRate;

                Assert.Equal(DefaultRateWindows.Length, rate.MovingRates.Length);
                Assert.Equal(DefaultRateWindows, rate.MovingRates.Select(x => x.WindowSeconds));
                Assert.Equal(Enumerable.Repeat(0.0, DefaultRateWindows.Length), rate.MovingRates.Select(x => x.Rate));
            }
        }

        [Theory]
        [InlineData(new object[] { new[] { "0.2", "0.3", "0.5" } })]
        [InlineData(new object[] { new[] { "0.8", "0.999" } })]
        public void CreatesTimerWithCustomPercentiles(string[] quantileStrings)
        {
            var quantiles = quantileStrings.Select(decimal.Parse).ToArray();

            using (Metrics.Configure().UsePercentiles(quantiles).Run())
            {
                var metric = MetricsRegistry.Timer("foo");
                Assert.Contains(metric, MetricsRegistry.ListMetrics());

                var value = metric.Value();
                Assert.Equal(quantiles.Length, value.Latencies.Percentiles.Length);
                Assert.Equal(quantiles, value.Latencies.Percentiles.Select(x => x.Quantile));
                Assert.Equal(Enumerable.Repeat(0L, quantiles.Length), value.Latencies.Percentiles.Select(x => x.Percentile));
            }
        }

        [Fact]
        public void CreatesTimerWithCustomSamplingReservoir()
        {
            var reservoirFactory = Mock.Of<ISamplingReservoirFactory>();
            var reservoir = Mock.Of<IReservoir<long>>();

            Func<double, long> percentileTransform = q => (long)((q + 1) * 100);
            var sampleSet = new FakeSampleSet<long>(percentileTransform, 1, 2, 3, 4, 5);
            var expectedPercentiles = DefaultQuantiles.Select(q => percentileTransform((double)q));

            Mock.Get(reservoirFactory).Setup(x => x.Create<long>(It.IsAny<IClock>()))
                .Returns(reservoir);
            Mock.Get(reservoir).Setup(x => x.GetSamples())
                .Returns(sampleSet);

            using (Metrics.Configure().UseHistogramSampling(reservoirFactory).Run())
            {
                var metric = MetricsRegistry.Timer("foo");

                var value = metric.Value().Latencies;
                Assert.Equal(DefaultQuantiles.Length, value.Percentiles.Length);
                Assert.Equal(1, value.Count);
                Assert.Equal(2, value.Max);
                Assert.Equal(3, value.Min);
                Assert.Equal(4, value.Mean);
                Assert.Equal(5, value.StdDev);
                Assert.Equal(expectedPercentiles, value.Percentiles.Select(x => x.Percentile));
            }
        }

        [Theory]
        [InlineData(new[] { 1, 2 })]
        [InlineData(new[] { 50, 75, 100 })]
        public void CreatesTimerCustomRateWindows(int[] rateWindows)
        {
            using (Metrics.Configure().UseRateWindows(rateWindows).Run())
            {
                var metric = MetricsRegistry.Timer("foo");

                var value = metric.Value().Rate;

                Assert.Equal(rateWindows.Length, value.MovingRates.Length);
                Assert.Equal(rateWindows, value.MovingRates.Select(x => x.WindowSeconds));
                Assert.Equal(Enumerable.Repeat(0.0, rateWindows.Length), value.MovingRates.Select(x => x.Rate));

                value = metric.Value().ErrorRate;

                Assert.Equal(rateWindows.Length, value.MovingRates.Length);
                Assert.Equal(rateWindows, value.MovingRates.Select(x => x.WindowSeconds));
                Assert.Equal(Enumerable.Repeat(0.0, rateWindows.Length), value.MovingRates.Select(x => x.Rate));
            }
        }

        [Fact]
        public void CreatesTimerCustomClock()
        {
            var clock = Mock.Of<IClock>();
            Mock.Get(clock).Setup(x => x.CurrentTimeNanoseconds())
                .Returns(() => DateTime.UtcNow.Ticks * 100);

            using (Metrics.Configure().UseClock(clock).Run())
            {
                MetricsRegistry.Timer("foo");
                Mock.Get(clock).VerifyAll();
            }
        }

        class FakeSampleSet<T> : ISampleSet<T>
            where T: struct, IComparable
        {
            private readonly Func<double, T> _percentileFunc;

            public T GetPercentile(double quantile)
            {
                return _percentileFunc(quantile);
            }

            public long Count { get; }
            public T Max { get; }
            public T Min { get; }
            public double Mean { get; }
            public double StdDev { get; }

            public FakeSampleSet(Func<double, T> percentileFunc, long count, T max, T min, double mean, double stdDev)
            {
                _percentileFunc = percentileFunc;
                Count = count;
                Max = max;
                Min = min;
                Mean = mean;
                StdDev = stdDev;
            }
        }
    }
}