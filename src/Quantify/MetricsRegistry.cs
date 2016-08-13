using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quantify.Metrics;
using Quantify.Metrics.Sampling;
using Quantify.Metrics.Time;

namespace Quantify
{
    public class MetricsRegistry
    {
        private static readonly ConcurrentDictionary<string, IMetric> Metrics = new ConcurrentDictionary<string, IMetric>();

        public static Counter Counter(string name)
        {
            return (Counter) Metrics.GetOrAdd(name, n => new Counter(n));
        }

        public static Gauge<int> Gauge(string name, Func<int> valueProvider)
        {
            return (Gauge<int>) Metrics.GetOrAdd(name, n => new Gauge<int>(n, valueProvider));
        }

        public static Gauge<long> Gauge(string name, Func<long> valueProvider)
        {
            return (Gauge<long>)Metrics.GetOrAdd(name, n => new Gauge<long>(n, valueProvider));
        }

        public static Gauge<double> Gauge(string name, Func<double> valueProvider)
        {
            return (Gauge<double>)Metrics.GetOrAdd(name, n => new Gauge<double>(n, valueProvider));
        }

        public static Gauge<decimal> Gauge(string name, Func<decimal> valueProvider)
        {
            return (Gauge<decimal>)Metrics.GetOrAdd(name, n => new Gauge<decimal>(n, valueProvider));
        }

        private static Histogram<T> CreateHistogram<T>(string name)
            where T : struct, IComparable
        {

            return (Histogram<T>)Metrics.GetOrAdd(name, n => new Histogram<T>(n, new ExponentiallyDecayingReservoir<T>(), new[] { 0.5m, 0.75m, 0.95m, 0.98m, 0.99m, 0.999m }));
        }

        public static Histogram<long> LongHistogram(string name)
        {
            return CreateHistogram<long>(name);
        }

        public static Histogram<double> DoubleHistogram(string name)
        {
            return CreateHistogram<double>(name);
        }

        public static Meter Meter(string name)
        {
            return (Meter) Metrics.GetOrAdd(name, n => new Meter(n, Clock.Default, new[] {60, 300, 900}));
        }

        public static Timer Timer(string name)
        {
            return (Timer) Metrics.GetOrAdd(name, n => new Timer(n, Clock.Default, new ExponentiallyDecayingReservoir<long>(), new[] { 0.5m, 0.75m, 0.95m, 0.98m, 0.99m, 0.999m }, new[] { 60, 300, 900 }));
        }

        public static IEnumerable<IMetric> ListMetrics()
        {
            return Metrics.Values;
        }

        public static void Reset()
        {
            Metrics.Clear();
        }
    }
}
