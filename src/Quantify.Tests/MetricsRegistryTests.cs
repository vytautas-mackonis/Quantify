using System;
using Xunit;
using Quantify;
using Quantify.Metrics;

namespace Quantify.Tests
{
    public class MetricsRegistryTests : IDisposable
    {
        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        public void CreatesCounter(string name)
        {
            var counter = MetricsRegistry.Counter(name);
            Assert.Contains(counter, MetricsRegistry.ListMetrics());
            Assert.Equal(name, counter.Name());
            Assert.Equal(0, counter.Value().Count);
        }

        [Theory]
        [InlineData("foo", 1)]
        [InlineData("bar", 3)]
        public void CreatesIntGauge(string name, int value)
        {
            var metric = MetricsRegistry.Gauge(name, () => value);
            Assert.Contains(metric, MetricsRegistry.ListMetrics());
            Assert.Equal(name, metric.Name());
            Assert.Equal(value, metric.Value().Value);
        }

        [Theory]
        [InlineData("foo", 1L)]
        [InlineData("bar", 3L)]
        public void CreatesLongGauge(string name, long value)
        {
            var metric = MetricsRegistry.Gauge(name, () => value);
            Assert.Contains(metric, MetricsRegistry.ListMetrics());
            Assert.Equal(name, metric.Name());
            Assert.Equal(value, metric.Value().Value);
        }

        [Theory]
        [InlineData("foo", 0.7)]
        [InlineData("bar", 14.45)]
        public void CreatesDoubleGauge(string name, double value)
        {
            var metric = MetricsRegistry.Gauge(name, () => value);
            Assert.Contains(metric, MetricsRegistry.ListMetrics());
            Assert.Equal(name, metric.Name());
            Assert.Equal(value, metric.Value().Value);
        }

        [Theory]
        [InlineData("foo", "0.7")]
        [InlineData("bar", "14.45")]
        public void CreatesDecimalGauge(string name, string value)
        {
            var decimalValue = decimal.Parse(value);
            var metric = MetricsRegistry.Gauge(name, () => decimalValue);
            Assert.Contains(metric, MetricsRegistry.ListMetrics());
            Assert.Equal(name, metric.Name());
            Assert.Equal(decimalValue, metric.Value().Value);
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        public void CreatesLongHistogram(string name)
        {
            var metric = MetricsRegistry.LongHistogram(name);
            Assert.Contains(metric, MetricsRegistry.ListMetrics());
            Assert.Equal(name, metric.Name());
            Assert.Equal(0, metric.Value().Count);
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        public void CreatesDoubleHistogram(string name)
        {
            var metric = MetricsRegistry.DoubleHistogram(name);
            Assert.Contains(metric, MetricsRegistry.ListMetrics());
            Assert.Equal(name, metric.Name());
            Assert.Equal(0, metric.Value().Count);
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        public void CreatesMeter(string name)
        {
            var metric = MetricsRegistry.Meter(name);
            Assert.Contains(metric, MetricsRegistry.ListMetrics());
            Assert.Equal(name, metric.Name());
            Assert.Equal(0, metric.Value().Count);
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("bar")]
        public void CreatesTimer(string name)
        {
            var metric = MetricsRegistry.Timer(name);
            Assert.Contains(metric, MetricsRegistry.ListMetrics());
            Assert.Equal(name, metric.Name());
            Assert.Equal(0, metric.Value().Latencies.Count);
        }

        public void Dispose()
        {
            MetricsRegistry.Reset();
        }
    }   
}